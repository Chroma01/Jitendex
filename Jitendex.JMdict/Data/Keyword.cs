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
using Jitendex.JMdict.Models;

namespace Jitendex.JMdict.Data;

internal static class KeywordData
{
    private const string C1 = nameof(IKeyword.Name);
    private const string C2 = nameof(IKeyword.Description);
    private const string C3 = nameof(IKeyword.IsCorrupt);

    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";

    public static async Task InsertKeywordsAsync<T>(this JmdictContext db, List<T> keywordList) where T : IKeyword
    {
        var sql =
        $"""
            INSERT INTO "{typeof(T).Name}"
            ("{C1}", "{C2}", "{C3}") VALUES
            ( {P1} ,  {P2} ,  {P3} );
        """;

        foreach (var keyword in keywordList)
        {
            var parameters = new SqliteParameter[]
            {
                new(P1, keyword.Name),
                new(P2, keyword.Description),
                new(P3, keyword.IsCorrupt),
            };
            await db.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}
