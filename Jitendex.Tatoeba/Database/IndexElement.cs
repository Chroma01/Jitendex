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
using Jitendex.Tatoeba.Models;
using Jitendex.SQLite;

namespace Jitendex.Tatoeba.Database;

internal static class IndexElementData
{
    // Column names
    private const string C1 = nameof(IndexElement.SentenceId);
    private const string C2 = nameof(IndexElement.IndexOrder);
    private const string C3 = nameof(IndexElement.Order);
    private const string C4 = nameof(IndexElement.Headword);
    private const string C5 = nameof(IndexElement.Reading);
    private const string C6 = nameof(IndexElement.EntryId);
    private const string C7 = nameof(IndexElement.SenseNumber);
    private const string C8 = nameof(IndexElement.SentenceForm);
    private const string C9 = nameof(IndexElement.IsPriority);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";
    private const string P4 = $"@{C4}";
    private const string P5 = $"@{C5}";
    private const string P6 = $"@{C6}";
    private const string P7 = $"@{C7}";
    private const string P8 = $"@{C8}";
    private const string P9 = $"@{C9}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(IndexElement)}"
        ("{C1}", "{C2}", "{C3}", "{C4}", "{C5}", "{C6}", "{C7}", "{C8}", "{C9}") VALUES
        ( {P1} ,  {P2} ,  {P3} ,  {P4} ,  {P5} ,  {P6} ,  {P7} ,  {P8} ,  {P9} );
        """;

    public static async Task InsertIndexElementsAsync(this Context db, IEnumerable<IndexElement> elements)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = InsertSql;
        foreach (var element in elements)
        {
            command.Parameters.AddRange(new SqliteParameter[]
            {
                new(P1, element.SentenceId),
                new(P2, element.IndexOrder),
                new(P3, element.Order),
                new(P4, element.Headword),
                new(P5, element.Reading.Nullable()),
                new(P6, element.EntryId.Nullable()),
                new(P7, element.SenseNumber.Nullable()),
                new(P8, element.SentenceForm.Nullable()),
                new(P9, element.IsPriority),
            });
            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
        }
    }
}
