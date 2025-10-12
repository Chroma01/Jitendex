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
using Jitendex.JMdict.Models.EntryElements;

namespace Jitendex.JMdict.Data.EntryElements;

internal static class KanjiFormReadingData
{
    // Column names
    private const string C1 = nameof(Reading.KanjiForms) + nameof(Reading.EntryId);
    private const string C2 = nameof(Reading.KanjiForms) + nameof(Reading.Order);
    private const string C3 = nameof(KanjiForm.Readings) + nameof(KanjiForm.EntryId);
    private const string C4 = nameof(KanjiForm.Readings) + nameof(KanjiForm.Order);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";
    private const string P4 = $"@{C4}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(KanjiForm)}{nameof(Reading)}"
        ("{C1}", "{C2}", "{C3}", "{C4}") VALUES
        ( {P1} ,  {P2} ,  {P3} ,  {P4} );
        """;

    public static async Task InsertKanjiFormReadingJoins(this JmdictContext db, List<KanjiForm> kanjiForms)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = InsertSql;

        foreach (var kanjiForm in kanjiForms)
        {
            foreach (var reading in kanjiForm.Readings)
            {
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new(P1, kanjiForm.EntryId),
                    new(P2, kanjiForm.Order),
                    new(P3, reading.EntryId),
                    new(P4, reading.Order),
                });

                await command.ExecuteNonQueryAsync();
                command.Parameters.Clear();
            }
        }
    }
}
