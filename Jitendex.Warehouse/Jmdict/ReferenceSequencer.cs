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
    private record ReferenceText(string text1, string? text2);

    public static void FixCrossReferences(List<Entry> entries, Dictionary<string, int> cache)
    {
        var referenceTextToEntries = ReferenceTextToEntries(entries);

        var crossReferences = entries
            .SelectMany(e => e.Senses)
            .SelectMany(s => s.CrossReferences);

        foreach (var xref in crossReferences)
        {
            var key = new ReferenceText(xref.RefText1, xref.RefText2);
            var possibleTargetEntries = referenceTextToEntries[key]
                .Where(e =>
                    e.Id != xref.EntryId &&  // Entries cannot reference themselves.
                    e.CorpusId == xref.Sense.Entry.CorpusId &&  // Assume references are within same corpus.
                    e.Senses.Count >= xref.RefSenseOrder)  // Referenced entry must contain the referenced sense ID.
                .ToList();

            var targetEntry = FindTargetEntry(possibleTargetEntries, cache, xref.RawKey());

            xref.RefEntryId = targetEntry.Id;
            xref.RefSense = targetEntry.Senses
                .Where(s => s.Order == xref.RefSenseOrder).First();
            xref.RefSense.ReverseCrossReferences.Add(xref);

            if (targetEntry.KanjiForms.All(k => k.IsHidden()))
            {
                string searchText;
                if (targetEntry.KanjiForms.Any(k => k.Text == xref.RefText1))
                {
                    searchText = targetEntry.Readings
                        .Where(r => !r.IsHidden())
                        .First().Text;
                    Console.WriteLine($"Entry {xref.EntryId} has a reference to hidden kanji form {xref.RefText1} in entry {targetEntry.Id}");
                }
                else
                {
                    searchText = xref.RefText1;
                }

                var refReading = targetEntry.Readings
                    .Where(b => b.Text == searchText).First();

                if (refReading.IsHidden())
                {
                    Console.WriteLine($"Entry {xref.EntryId} has a reference to hidden reading {xref.RefText1} in entry {targetEntry.Id}");
                    refReading = targetEntry.Readings
                        .Where(r => !r.IsHidden()).First();
                }

                xref.RefReading = refReading;
                xref.RefReadingOrder = refReading.Order;
            }
            else if (targetEntry.KanjiForms.Any(k => k.Text == xref.RefText1))
            {
                var refKanjiForm = targetEntry.KanjiForms
                    .Where(k => k.Text == xref.RefText1).First();

                if (refKanjiForm.IsHidden())
                {
                    Console.WriteLine($"Entry {xref.EntryId} has a reference to hidden kanji form {xref.RefText1} in entry {targetEntry.Id}");
                    refKanjiForm = targetEntry.KanjiForms
                        .Where(k => !k.IsHidden()).First();
                }

                xref.RefKanjiForm = refKanjiForm;
                xref.RefKanjiFormOrder = refKanjiForm.Order;

                var refReading = xref.RefText2 is null ?
                    refKanjiForm.ReadingBridges.First().Reading :
                    refKanjiForm.ReadingBridges
                        .Where(b => b.Reading.Text == xref.RefText2)
                        .First().Reading;

                xref.RefReading = refReading;
                xref.RefReadingOrder = refReading.Order;
            }
            else
            {
                var refReading = targetEntry.Readings
                    .Where(b => b.Text == xref.RefText1).First();

                if (refReading.IsHidden())
                {
                    Console.WriteLine($"Entry {xref.EntryId} has a reference to hidden form {xref.RefText1} in entry {targetEntry.Id}");
                    refReading = targetEntry.Readings
                        .Where(r => !r.IsHidden()).First();
                }

                xref.RefReading = refReading;
                xref.RefReadingOrder = refReading.Order;
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
        if (possibleTargetEntries.Count == 0)
        {
            throw new Exception($"No entries found for cross reference `{cacheKey}`");
        }
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
            return possibleTargetEntries.First();
        }

        Console.WriteLine($"Reference could refer to {possibleTargetEntries.Count} possible entries:");
        Console.WriteLine($"\t\"{cacheKey}\": {string.Join(" || ", possibleTargetEntries.Select(e => e.Id.ToString()).ToList())},");
        return possibleTargetEntries.First();
    }
}
