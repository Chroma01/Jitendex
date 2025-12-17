/*
Copyright (c) 2025 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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
using Jitendex.JMdict.Models.EntryElements.SenseElements;

namespace Jitendex.JMdict.Database.EntryElements.SenseElements;

internal static class ExampleData
{
    // Column names
    private const string C1 = nameof(Example.EntryId);
    private const string C2 = nameof(Example.SenseOrder);
    private const string C3 = nameof(Example.Order);
    private const string C4 = nameof(Example.SourceTypeName);
    private const string C5 = nameof(Example.SourceKey);
    private const string C6 = nameof(Example.Keyword);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";
    private const string P4 = $"@{C4}";
    private const string P5 = $"@{C5}";
    private const string P6 = $"@{C6}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(Example)}"
        ("{C1}", "{C2}", "{C3}", "{C4}", "{C5}", "{C6}") VALUES
        ( {P1} ,  {P2} ,  {P3} ,  {P4} ,  {P5} ,  {P6} );
        """;

    public static async Task InsertExamples(this Context db, List<Example> examples)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = InsertSql;

        foreach (var example in examples)
        {
            command.Parameters.AddRange(new SqliteParameter[]
            {
                new(P1, example.EntryId),
                new(P2, example.SenseOrder),
                new(P3, example.Order),
                new(P4, example.SourceTypeName),
                new(P5, example.SourceKey),
                new(P6, example.Keyword),
            });

            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
        }
    }
}
