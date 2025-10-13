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
using Jitendex.Kanjidic2.Models.EntryElements;
using Jitendex.SQLite;

namespace Jitendex.Kanjidic2.Database.EntryElements;

internal static class DictionaryData
{
    // Column names
    private const string C1 = nameof(Dictionary.UnicodeScalarValue);
    private const string C2 = nameof(Dictionary.Order);
    private const string C3 = nameof(Dictionary.Text);
    private const string C4 = nameof(Dictionary.TypeName);
    private const string C5 = nameof(Dictionary.Volume);
    private const string C6 = nameof(Dictionary.Page);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";
    private const string P4 = $"@{C4}";
    private const string P5 = $"@{C5}";
    private const string P6 = $"@{C6}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(Dictionary)}"
        ("{C1}", "{C2}", "{C3}", "{C4}", "{C5}", "{C6}") VALUES
        ( {P1} ,  {P2} ,  {P3} ,  {P4} ,  {P5} ,  {P6} );
        """;

    public static async Task InsertDictionariesAsync(this Context db, List<Dictionary> dictionaries)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = InsertSql;

        foreach (var dictionary in dictionaries)
        {
            command.Parameters.AddRange(new SqliteParameter[]
            {
                new(P1, dictionary.UnicodeScalarValue),
                new(P2, dictionary.Order),
                new(P3, dictionary.Text),
                new(P4, dictionary.TypeName),
                new(P5, dictionary.Volume.Nullable()),
                new(P6, dictionary.Page.Nullable()),
            });

            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
        }
    }
}
