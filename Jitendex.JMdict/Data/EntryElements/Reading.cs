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
using Jitendex.JMdict.Data.EntryElements.ReadingElements;
using Jitendex.JMdict.Models.EntryElements;
using Jitendex.JMdict.Models.EntryElements.ReadingElements;

namespace Jitendex.JMdict.Data.EntryElements;

internal static class ReadingData
{
    // Column names
    private const string C1 = nameof(Reading.EntryId);
    private const string C2 = nameof(Reading.Order);
    private const string C3 = nameof(Reading.Text);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(Reading)}"
        ("{C1}", "{C2}", "{C3}") VALUES
        ( {P1} ,  {P2} ,  {P3} );
        """;

    public static async Task InsertReadings(this JmdictContext db, List<Reading> readings)
    {
        var allInfos = new List<ReadingInfo>(readings.Count / 10);
        var allPriorities = new List<ReadingPriority>(readings.Count / 3);

        await using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = InsertSql;

            foreach (var reading in readings)
            {
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new(P1, reading.EntryId),
                    new(P2, reading.Order),
                    new(P3, reading.Text),
                });

                var commandExecution = command.ExecuteNonQueryAsync();

                allInfos.AddRange(reading.Infos);
                allPriorities.AddRange(reading.Priorities);

                await commandExecution;
                command.Parameters.Clear();
            }
        }

        // Child elements
        await db.InsertReadingInfo(allInfos);
        await db.InsertReadingPriority(allPriorities);
    }
}
