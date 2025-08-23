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

using Jitendex.Warehouse.Jmdict.Models;

namespace Jitendex.Warehouse.Jmdict;

internal static class ReferenceSequencer
{
    private record ReferenceText(string text1, string? text2)
    {
        public override string ToString()
            => text2 is null ? text1 : $"{text1}【{text2}】";
    }

    private record SpellingId(int ReadingOrder, int? KanjiFormOrder);

    public static void FixCrossReferences(List<Entry> entries, Dictionary<string, int> cache)
    {
        var referenceTextToEntries = ReferenceTextToEntries(entries);

        var allCrossReferences = entries
            .SelectMany(e => e.Senses)
            .SelectMany(s => s.CrossReferences);

        foreach (var xref in allCrossReferences)
        {
            var key = new ReferenceText(xref.RefText1, xref.RefText2);
            var possibleTargetEntries = referenceTextToEntries[key]
                .Where(e =>
                    e.Id != xref.EntryId &&  // Entries cannot reference themselves.
                    e.CorpusId == xref.Sense.Entry.CorpusId &&  // Assume references are within same corpus.
                    e.Senses.Count >= xref.RefSenseOrder)  // Referenced entry must contain the referenced sense number.
                .ToList();

            var targetEntry = FindTargetEntry(possibleTargetEntries, cache, xref.RawKey());

            // Assign Sense foreign key.
            xref.RefEntryId = targetEntry.Id;
            xref.RefSense = targetEntry.Senses
                .Where(s => s.Order == xref.RefSenseOrder).First();
            xref.RefSense.ReverseCrossReferences.Add(xref);

            // Assign Reading and KanjiForm foreign keys.
            var validSpellingIds = targetEntry.ValidSpellings();
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
                Console.WriteLine($"Reference display text `{key}` in entry {xref.EntryId} is an invalid spelling for entry {targetEntry.Id}");
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
            foreach (var referenceText in entry.ReferenceTexts())
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

    private static IEnumerable<ReferenceText> ReferenceTexts(this Entry entry)
    {
        foreach (var reading in entry.Readings)
        {
            yield return new ReferenceText(reading.Text, null);
        }
        foreach (var kanjiForm in entry.KanjiForms)
        {
            yield return new ReferenceText(kanjiForm.Text, null);
            foreach (var reading in entry.Readings)
            {
                yield return new ReferenceText(kanjiForm.Text, reading.Text);
            }
        }
    }

    private static Entry FindTargetEntry(List<Entry> possibleTargetEntries, Dictionary<string, int> cache, string cacheKey)
    {
        if (possibleTargetEntries.Count == 1)
        {
            return possibleTargetEntries.First();
        }

        // If there are multiple target entries, then the reference is ambiguous.
        // The correct entry ID must be recorded in the cache.
        if (cache.TryGetValue(cacheKey, out int targetEntryId))
        {
            var targetEntry = possibleTargetEntries
                .Where(e => e.Id == targetEntryId)
                .FirstOrDefault();
            if (targetEntry is not null)
                return targetEntry;

            Console.WriteLine($"Cached ID `{targetEntryId}` is invalid for reference `{cacheKey}`");
        }

        if (possibleTargetEntries.Count == 0)
        {
            throw new Exception($"No entries found for cross reference `{cacheKey}`");
        }

        Console.WriteLine($"Reference could refer to {possibleTargetEntries.Count} possible entries:");
        Console.WriteLine($"\t\"{cacheKey}\": {string.Join(" || ", possibleTargetEntries.Select(e => e.Id.ToString()).ToList())},");
        return possibleTargetEntries.First();
    }

    private static Dictionary<ReferenceText, SpellingId> ValidSpellings(this Entry entry)
    {
        var map = new Dictionary<ReferenceText, SpellingId>();
        ReferenceText referenceText;

        foreach (var kanjiForm in entry.KanjiForms.Where(k => !k.IsHidden()))
        {
            referenceText = new ReferenceText(kanjiForm.Text, null);
            map[referenceText] = new SpellingId
            (
                kanjiForm.ReadingBridges.First().Reading.Order,
                kanjiForm.Order
            );
            foreach (var bridge in kanjiForm.ReadingBridges)
            {
                referenceText = new ReferenceText(kanjiForm.Text, bridge.Reading.Text);
                map[referenceText] = new SpellingId(bridge.Reading.Order, kanjiForm.Order);
            }
        }
        foreach (var reading in entry.Readings.Where(r => !r.IsHidden()))
        {
            referenceText = new ReferenceText(reading.Text, null);
            map[referenceText] = new SpellingId(reading.Order, null);
        }
        return map;
    }
}
