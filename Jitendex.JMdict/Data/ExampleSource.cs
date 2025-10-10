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

internal static class ExampleSourceData
{
    // Column names
    private const string C1 = nameof(ExampleSource.TypeName);
    private const string C2 = nameof(ExampleSource.OriginKey);
    private const string C3 = nameof(ExampleSource.Text);
    private const string C4 = nameof(ExampleSource.Translation);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";
    private const string P4 = $"@{C4}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(ExampleSource)}"
        ("{C1}", "{C2}", "{C3}", "{C4}") VALUES
        ( {P1} ,  {P2} ,  {P3} ,  {P4} );
        """;

    public static async Task InsertExampleSourcesAsync(this JmdictContext db, List<ExampleSource> exampleSources)
    {
        foreach (var exampleSource in exampleSources)
        {
            var parameters = new SqliteParameter[]
            {
                new(P1, exampleSource.TypeName),
                new(P2, exampleSource.OriginKey),
                new(P3, exampleSource.Text),
                new(P4, exampleSource.Translation),
            };
            await db.Database.ExecuteSqlRawAsync(InsertSql, parameters);
        }
    }
}
