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

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Jitendex.Kanjidic2.Database.EntryElements;
using Jitendex.Kanjidic2.Models;
using Jitendex.Kanjidic2.Models.EntryElements;

namespace Jitendex.Kanjidic2.Database;

internal static class EntryData
{
    // Column names
    private const string C1 = nameof(Entry.Character);
    private const string C2 = nameof(Entry.Grade);
    private const string C3 = nameof(Entry.Frequency);
    private const string C4 = nameof(Entry.JlptLevel);
    private const string C5 = nameof(Entry.IsKokuji);
    private const string C6 = nameof(Entry.IsGhost);
    private const string C7 = nameof(Entry.IsCorrupt);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";
    private const string P4 = $"@{C4}";
    private const string P5 = $"@{C5}";
    private const string P6 = $"@{C6}";
    private const string P7 = $"@{C7}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(Entry)}"
        ("{C1}", "{C2}", "{C3}", "{C4}", "{C5}", "{C6}", "{C7}") VALUES
        ( {P1} ,  {P2} ,  {P3} ,  {P4} ,  {P5} ,  {P6} ,  {P7} );
        """;

    public static async Task InsertEntriesAsync(this Context db, List<Entry> entries)
    {
        var allCodepoints = new List<Codepoint>(entries.Count * 3);
        var allDictionaries = new List<Dictionary>(entries.Count * 6);
        var allMeanings = new List<Meaning>(entries.Count * 2);
        var allNanori = new List<Nanori>(entries.Count / 2);
        var allQueryCodes = new List<QueryCode>(entries.Count * 3);
        var allRadicals = new List<Radical>(entries.Count * 2);
        var allRadicalNames = new List<RadicalName>(200);
        var allReadings = new List<Reading>(entries.Count * 7);
        var allStrokeCounts = new List<StrokeCount>(entries.Count * 2);
        var allVariants = new List<Variant>(entries.Count / 2);

        await using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = InsertSql;

            foreach (var entry in entries)
            {
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new(P1, entry.Character),
                    new(P2, entry.Grade is null ? DBNull.Value : entry.Grade),
                    new(P3, entry.Frequency is null ? DBNull.Value : entry.Frequency),
                    new(P4, entry.JlptLevel is null ? DBNull.Value : entry.JlptLevel),
                    new(P5, entry.IsKokuji),
                    new(P6, entry.IsGhost),
                    new(P7, entry.IsCorrupt),
                });

                var commandExecution = command.ExecuteNonQueryAsync();

                allCodepoints.AddRange(entry.Codepoints);
                allDictionaries.AddRange(entry.Dictionaries);
                allMeanings.AddRange(entry.Meanings);
                allNanori.AddRange(entry.Nanoris);
                allQueryCodes.AddRange(entry.QueryCodes);
                allRadicals.AddRange(entry.Radicals);
                allRadicalNames.AddRange(entry.RadicalNames);
                allReadings.AddRange(entry.Readings);
                allStrokeCounts.AddRange(entry.StrokeCounts);
                allVariants.AddRange(entry.Variants);

                await commandExecution;
                command.Parameters.Clear();
            }
        }

        await db.InsertCodepointsAsync(allCodepoints);
        await db.InsertDictionariesAsync(allDictionaries);
        await db.InsertMeaningsAsync(allMeanings);
        await db.InsertNanorisAsync(allNanori);
        await db.InsertQueryCodesAsync(allQueryCodes);
        await db.InsertRadicalsAsync(allRadicals);
        await db.InsertRadicalNamesAsync(allRadicalNames);
        await db.InsertReadingsAsync(allReadings);
        await db.InsertStrokeCountsAsync(allStrokeCounts);
        await db.InsertVariantsAsync(allVariants);
    }
}
