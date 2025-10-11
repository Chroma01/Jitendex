/*
Copyright (c) 2025 Stephen Kraus

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the
terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along
with Jitendex. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Jitendex.JMdict.Models;
using Jitendex.JMdict.Models.EntryElements;
using Jitendex.JMdict.Models.EntryElements.SenseElements;

namespace Jitendex.JMdict.Readers;

internal partial class ReferenceSequencer
{
    private readonly ILogger<ReferenceSequencer> _logger;
    private readonly CrossReferenceIds _crossReferenceIds;
    private readonly Dictionary<string, object> _usedDisambiguations = [];

    public ReferenceSequencer(ILogger<ReferenceSequencer> logger, CrossReferenceIds crossReferenceIds) =>
        (_logger, _crossReferenceIds) =
        (@logger, @crossReferenceIds);

    private readonly record struct ReferenceText(string Text1, string? Text2)
    {
        public override string ToString() => Text2 is not null
            ? $"{Text1}【{Text2}】"
            : Text1;
    }

    private readonly record struct SpellingId(int ReadingOrder, int? KanjiFormOrder);

    public async Task FixCrossReferencesAsync(List<Entry> entries)
    {
        var loadTask = _crossReferenceIds.LoadAsync();

        var referenceTextToEntries = ReferenceTextToEntries(entries);

        var allCrossReferences = entries
            .SelectMany(static entry => entry.Senses)
            .SelectMany(static sense => sense.RawCrossReferences);

        var disambiguationCache = await loadTask;

        foreach (var xref in allCrossReferences)
        {
            var key = new ReferenceText(xref.RefText1, xref.RefText2);
            var refEntry = GetReferencedEntry(entries, referenceTextToEntries, key, disambiguationCache, xref);
            var refSense = GetReferencedSense(refEntry, xref.RefSenseOrder);
            var (refReading, refKanjiForm) = GetReferencedReadingKanjiForm(refEntry, xref, key);
            var newXref = new CrossReference
            {
                EntryId = xref.EntryId,
                SenseOrder = xref.SenseOrder,
                Order = xref.Order,
                TypeName = xref.TypeName,
                Sense = xref.Sense,
                Type = xref.Type,
                RefEntryId = refEntry.Id,
                RefSenseOrder = xref.RefSenseOrder,
                RefReadingOrder = refReading.Order,
                RefKanjiFormOrder = refKanjiForm?.Order,
                RefSense = refSense,
                RefReading = refReading,
                RefKanjiForm = refKanjiForm,
            };
            xref.Sense.CrossReferences.Add(newXref);
            refSense.ReverseCrossReferences.Add(newXref);
        }

        await _crossReferenceIds.WriteAsync(_usedDisambiguations);
    }

    private Entry GetReferencedEntry(
        in List<Entry> entries,
        in ReadOnlyDictionary<ReferenceText, ImmutableArray<Entry>> referenceTextToEntries,
        in ReferenceText key,
        in ReadOnlyDictionary<string, int> disambiguationCache,
        in RawCrossReference xref)
    {
        var (id, corpusId, senseNumber) = (
            xref.EntryId,
            xref.Sense.Entry.CorpusId,
            xref.RefSenseOrder);

        var possibleTargetEntries =
            referenceTextToEntries.TryGetValue(key, out ImmutableArray<Entry> keyEntries)
            ? keyEntries.Where(e
                    => e.Id != id                      // Entries cannot reference themselves.
                    && e.CorpusId == corpusId          // Assume references are within same corpus.
                    && e.Senses.Count >= senseNumber)  // Referenced entry must contain the referenced sense number.
                .ToList()
            : [];

        if (possibleTargetEntries is [])
        {
            LogImpossibleReference(xref.RawKey());
            xref.Sense.Entry.IsCorrupt = true;
            possibleTargetEntries = [entries.Last()];
        }

        return possibleTargetEntries.Count == 1
            ? possibleTargetEntries.First()
            : FindTargetEntry(possibleTargetEntries, xref, disambiguationCache);
    }

    private static Sense GetReferencedSense(in Entry entry, int refOrder) => entry
        .Senses
        .Where(sense => sense.Order == refOrder)
        .First();

    private (Reading, KanjiForm?) GetReferencedReadingKanjiForm(
        in Entry entry,
        in RawCrossReference xref,
        in ReferenceText key)
    {
        if (ValidSpellings(entry).TryGetValue(key, out SpellingId id))
        {
            var refReading = entry.Readings.Where(r => r.Order == id.ReadingOrder).First();
            var refKanjiForm = id.KanjiFormOrder is not null
                ? entry.KanjiForms.Where(k => k.Order == id.KanjiFormOrder).First()
                : null;
            return (refReading, refKanjiForm);
        }
        else
        {
            LogInvalidSpelling(key.ToString(), xref.EntryId, entry.Id);
            xref.Sense.Entry.IsCorrupt = true;

            var refReading = entry.Readings.Where(r => !r.IsHidden()).First();
            var refKanjiForm = refReading.KanjiFormBridges.FirstOrDefault()?.KanjiForm;
            return (refReading, refKanjiForm);
        }
    }

    /// <remarks>
    /// For JMdict dated 2025-10-11, there were 709,038 distinct possible reference texts.
    /// The corresponding entry arrays had an average length of 1.046.
    /// </remarks>
    private static ReadOnlyDictionary<ReferenceText, ImmutableArray<Entry>> ReferenceTextToEntries(in List<Entry> entries)
    {
        var dict = new Dictionary<ReferenceText, ImmutableArray<Entry>>();
        foreach (var entry in entries)
        {
            foreach (var referenceText in ReferenceTexts(entry))
            {
                if (dict.TryGetValue(referenceText, out ImmutableArray<Entry> values))
                {
                    dict[referenceText] = values.Add(entry);
                }
                else
                {
                    dict[referenceText] = [entry];
                }
            }
        }
        return dict.AsReadOnly();
    }

    private static IEnumerable<ReferenceText> ReferenceTexts(Entry entry)
    {
        foreach (var kanjiForm in entry.KanjiForms)
        {
            foreach (var reading in entry.Readings)
            {
                yield return new ReferenceText(kanjiForm.Text, reading.Text);
            }

            // References in Jmdict sometimes display only the kanji form without a reading.
            yield return new ReferenceText(kanjiForm.Text, null);
        }

        // It is also possible for references to only show the reading,
        // even if valid kanji forms are available.
        foreach (var reading in entry.Readings)
        {
            yield return new ReferenceText(reading.Text, null);
        }
    }

    /// <summary>
    /// If there are multiple target entries, then the reference is ambiguous.
    /// The correct entry ID must be recorded in the cache.
    /// </summary>
    private Entry FindTargetEntry(
        in List<Entry> possibleTargetEntries,
        in RawCrossReference xref,
        in ReadOnlyDictionary<string, int> disambiguationCache)
    {
        var cacheKey = xref.RawKey();

        var targetEntry =
            disambiguationCache.TryGetValue(cacheKey, out int targetEntryId)
            ? possibleTargetEntries
                .Where(e => e.Id == targetEntryId)
                .FirstOrDefault()
            : null;

        if (targetEntry is not null)
        {
            _usedDisambiguations[cacheKey] = targetEntry.Id;
            return targetEntry;
        }

        xref.Sense.Entry.IsCorrupt = true;

        var possibleTargetEntryIds = possibleTargetEntries
            .Select(static e => e.Id)
            .ToArray();

        _usedDisambiguations[cacheKey] = possibleTargetEntryIds;

        LogAmbiguousReference
        (
            cacheKey,
            possibleTargetEntryIds.Length,
            possibleTargetEntryIds
        );

        return possibleTargetEntries.First();
    }

    private static Dictionary<ReferenceText, SpellingId> ValidSpellings(in Entry entry)
    {
        var map = new Dictionary<ReferenceText, SpellingId>();
        ReferenceText referenceText;

        foreach (var kanjiForm in entry.KanjiForms.Where(static k => !k.IsHidden()))
        {
            foreach (var bridge in kanjiForm.ReadingBridges)
            {
                referenceText = new ReferenceText(kanjiForm.Text, bridge.Reading.Text);
                map[referenceText] = new SpellingId(bridge.Reading.Order, kanjiForm.Order);
            }

            // Sometimes references in Jmdict display only the kanji form without
            // a reading. In these cases, we'll map to the first valid reading.
            referenceText = new ReferenceText(kanjiForm.Text, null);

            map[referenceText] = new SpellingId
            (
                kanjiForm.ReadingBridges.First().Reading.Order,
                kanjiForm.Order
            );
        }

        // It is also possible for references to only show the reading,
        // even if valid kanji forms are available.
        foreach (var reading in entry.Readings.Where(static r => !r.IsHidden()))
        {
            referenceText = new ReferenceText(reading.Text, null);
            map[referenceText] = new SpellingId(reading.Order, null);
        }
        return map;
    }

    [LoggerMessage(LogLevel.Warning,
    "Reference spelling `{Spelling}` in entry {EntryId} is invalid for referenced entry {TargetEntryId}")]
    private partial void LogInvalidSpelling(string spelling, int entryId, int targetEntryId);

    [LoggerMessage(LogLevel.Warning,
    "Reference `{CacheKey}` could refer to {Count} possible entries: {EntryIds}")]
    private partial void LogAmbiguousReference(string cacheKey, int count, int[] entryIds);

    [LoggerMessage(LogLevel.Warning,
    "Reference `{CacheKey}` refers to an entry that does not exist.")]
    private partial void LogImpossibleReference(string cacheKey);
}
