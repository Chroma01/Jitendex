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

using Microsoft.Extensions.Logging;
using Jitendex.Warehouse.Jmdict.Models;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Warehouse.Jmdict;

internal partial class ReferenceSequencer
{
    private readonly Dictionary<string, int> _disambiguationCache;
    private readonly ILogger<ReferenceSequencer> _logger;

    public ReferenceSequencer(Dictionary<string, int> disambiguationCache, ILogger<ReferenceSequencer> logger) =>
        (_disambiguationCache, _logger) =
        (@disambiguationCache, @logger);

    private record ReferenceText(string Text1, string? Text2)
    {
        public override string ToString() =>
            Text2 is null ? Text1 : $"{Text1}【{Text2}】";
    }

    private record SpellingId(int ReadingOrder, int? KanjiFormOrder);

    public void FixCrossReferences(List<Entry> entries)
    {
        var referenceTextToEntries = ReferenceTextToEntries(entries);

        var allCrossReferences = entries
            .SelectMany(e => e.Senses)
            .SelectMany(s => s.CrossReferences);

        foreach (var xref in allCrossReferences)
        {
            var key = new ReferenceText(xref.RefText1, xref.RefText2);
            List<Entry> possibleTargetEntries;

            if (referenceTextToEntries.TryGetValue(key, out List<Entry>? keyEntries))
            {
                possibleTargetEntries = keyEntries
                    .Where(e =>
                        e.Id != xref.EntryId &&  // Entries cannot reference themselves.
                        e.CorpusId == xref.Sense.Entry.CorpusId &&  // Assume references are within same corpus.
                        e.Senses.Count >= xref.RefSenseOrder)  // Referenced entry must contain the referenced sense number.
                    .ToList();
            }
            else
            {
                possibleTargetEntries = [];
            }

            var targetEntry = FindTargetEntry(possibleTargetEntries, xref) ?? entries.Last();

            // Assign Sense foreign key.
            xref.RefEntryId = targetEntry.Id;
            xref.RefSense = targetEntry.Senses
                .Where(s => s.Order == xref.RefSenseOrder).First();
            xref.RefSense.ReverseCrossReferences.Add(xref);

            // Assign Reading and KanjiForm foreign keys.
            var validSpellingIds = ValidSpellings(targetEntry);
            if (validSpellingIds.TryGetValue(key, out SpellingId? id))
            {
                xref.RefKanjiFormOrder = id.KanjiFormOrder;
                xref.RefKanjiForm = targetEntry.KanjiForms
                    .Where(k => k.Order == id.KanjiFormOrder).FirstOrDefault();

                xref.RefReadingOrder = id.ReadingOrder;
                xref.RefReading = targetEntry.Readings
                    .Where(r => r.Order == id.ReadingOrder).First();
            }
            else
            {
                LogInvalidSpelling(key.ToString(), xref.EntryId, targetEntry.Id);
                xref.IsCorrupt = true;

                xref.RefReading = targetEntry.Readings
                    .Where(r => !r.IsHidden()).First();
                xref.RefReadingOrder = xref.RefReading.Order;

                xref.RefKanjiForm = xref.RefReading.KanjiFormBridges.FirstOrDefault()?.KanjiForm;
                xref.RefKanjiFormOrder = xref.RefKanjiForm?.Order;
            }
        }
    }

    private static Dictionary<ReferenceText, List<Entry>> ReferenceTextToEntries(List<Entry> entries)
    {
        var map = new Dictionary<ReferenceText, List<Entry>>();
        foreach (var entry in entries)
        {
            foreach (var referenceText in ReferenceTexts(entry))
            {
                if (map.TryGetValue(referenceText, out List<Entry>? values))
                {
                    values.Add(entry);
                }
                else
                {
                    map[referenceText] = [entry];
                }
            }
        }
        return map;
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

    private Entry? FindTargetEntry(List<Entry> possibleTargetEntries, CrossReference xref)
    {
        if (possibleTargetEntries.Count == 1)
            return possibleTargetEntries.First();

        // If there are multiple target entries, then the reference is ambiguous.
        // The correct entry ID must be recorded in the cache.
        var cacheKey = xref.RawKey();
        if (_disambiguationCache.TryGetValue(cacheKey, out int targetEntryId))
        {
            var targetEntry = possibleTargetEntries
                .Where(e => e.Id == targetEntryId)
                .FirstOrDefault();
            if (targetEntry is not null)
                return targetEntry;
            else
                LogInvalidCacheId(cacheKey, targetEntryId);
        }

        xref.IsCorrupt = true;

        if (possibleTargetEntries.Count == 0)
        {
            LogImpossibleReference(cacheKey);
            return null;
        }

        LogAmbiguousReference
        (
            cacheKey,
            possibleTargetEntries.Count,
            possibleTargetEntries.Select(e => e.Id).ToArray()
        );
        return possibleTargetEntries.First();
    }

    private static Dictionary<ReferenceText, SpellingId> ValidSpellings(Entry entry)
    {
        var map = new Dictionary<ReferenceText, SpellingId>();
        ReferenceText referenceText;

        foreach (var kanjiForm in entry.KanjiForms.Where(k => !k.IsHidden()))
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
        foreach (var reading in entry.Readings.Where(r => !r.IsHidden()))
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
    "Cached ID `{TargetEntryId}` is invalid for reference `{CacheKey}`")]
    private partial void LogInvalidCacheId(string cacheKey, int targetEntryId);

    [LoggerMessage(LogLevel.Warning,
    "Reference `{CacheKey}` refers to an entry that does not exist.")]
    private partial void LogImpossibleReference(string cacheKey);
}
