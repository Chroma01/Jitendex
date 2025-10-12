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
using Jitendex.JMdict.Database.EntryElements.KanjiFormElements;
using Jitendex.JMdict.Models.EntryElements;
using Jitendex.JMdict.Models.EntryElements.KanjiFormElements;

namespace Jitendex.JMdict.Database.EntryElements;

internal static class KanjiFormData
{
    // Column names
    private const string C1 = nameof(KanjiForm.EntryId);
    private const string C2 = nameof(KanjiForm.Order);
    private const string C3 = nameof(KanjiForm.Text);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(KanjiForm)}"
        ("{C1}", "{C2}", "{C3}") VALUES
        ( {P1} ,  {P2} ,  {P3} );
        """;

    public static async Task InsertKanjiForms(this Context db, List<KanjiForm> kanjiForms)
    {
        var allInfos = new List<KanjiFormInfo>(kanjiForms.Count / 10);
        var allPriorities = new List<KanjiFormPriority>(kanjiForms.Count / 3);

        await using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = InsertSql;

            foreach (var kanjiForm in kanjiForms)
            {
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new(P1, kanjiForm.EntryId),
                    new(P2, kanjiForm.Order),
                    new(P3, kanjiForm.Text),
                });

                var commandExecution = command.ExecuteNonQueryAsync();

                allInfos.AddRange(kanjiForm.Infos);
                allPriorities.AddRange(kanjiForm.Priorities);

                await commandExecution;
                command.Parameters.Clear();
            }
        }

        // Child elements
        await db.InsertKanjiFormReadingJoins(kanjiForms);
        await db.InsertKanjiFormInfo(allInfos);
        await db.InsertKanjiFormPriority(allPriorities);
    }
}
