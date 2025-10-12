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
using Jitendex.JMdict.Database.EntryElements.SenseElements;
using Jitendex.JMdict.Models.EntryElements;
using Jitendex.JMdict.Models.EntryElements.SenseElements;

namespace Jitendex.JMdict.Database.EntryElements;

internal static class SenseData
{
    // Column names
    private const string C1 = nameof(Sense.EntryId);
    private const string C2 = nameof(Sense.Order);
    private const string C3 = nameof(Sense.Note);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(Sense)}"
        ("{C1}", "{C2}", "{C3}") VALUES
        ( {P1} ,  {P2} ,  {P3} );
        """;

    public static async Task InsertSenses(this JmdictContext db, List<Sense> senses)
    {
        var allCrossReferences = new List<CrossReference>(senses.Count / 5);
        var allDialects = new List<Dialect>(senses.Count / 100);
        var allExamples = new List<Example>(senses.Count / 5);
        var allFields = new List<Field>(senses.Count / 5);
        var allGlosses = new List<Gloss>(senses.Count * 2);
        var allKanjiFormRestrictions = new List<KanjiFormRestriction>(senses.Count / 100);
        var allLanguageSources = new List<LanguageSource>(senses.Count / 20);
        var allMiscs = new List<Misc>(senses.Count / 3);
        var allPartsOfSpeech = new List<PartOfSpeech>(senses.Count * 2);
        var allReadingRestrictions = new List<ReadingRestriction>(senses.Count / 100);

        await using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = InsertSql;

            foreach (var sense in senses)
            {
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new(P1, sense.EntryId),
                    new(P2, sense.Order),
                    new(P3, sense.Note is null ? DBNull.Value : sense.Note),
                });

                var commandExecution = command.ExecuteNonQueryAsync();

                allCrossReferences.AddRange(sense.CrossReferences);
                allDialects.AddRange(sense.Dialects);
                allExamples.AddRange(sense.Examples);
                allFields.AddRange(sense.Fields);
                allGlosses.AddRange(sense.Glosses);
                allKanjiFormRestrictions.AddRange(sense.KanjiFormRestrictions);
                allLanguageSources.AddRange(sense.LanguageSources);
                allMiscs.AddRange(sense.Miscs);
                allPartsOfSpeech.AddRange(sense.PartsOfSpeech);
                allReadingRestrictions.AddRange(sense.ReadingRestrictions);

                await commandExecution;
                command.Parameters.Clear();
            }
        }

        // Child elements
        await db.InsertCrossReferences(allCrossReferences);
        await db.InsertDialects(allDialects);
        await db.InsertExamples(allExamples);
        await db.InsertFields(allFields);
        await db.InsertGlosses(allGlosses);
        await db.InsertKanjiFormRestrictions(allKanjiFormRestrictions);
        await db.InsertLanguageSources(allLanguageSources);
        await db.InsertMiscs(allMiscs);
        await db.InsertPartsOfSpeech(allPartsOfSpeech);
        await db.InsertReadingRestrictions(allReadingRestrictions);
    }
}
