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

internal static class CorpusData
{
    private const string C1 = nameof(Corpus.Id);
    private const string C2 = nameof(Corpus.Name);

    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";

    private const string InsertSql =
    $"""
        INSERT INTO "{nameof(Corpus)}"
        ("{C1}", "{C2}") VALUES
        ( {P1} ,  {P2} );
    """;

    public static async Task InsertCorporaAsync(this JmdictContext db, List<Corpus> corpora)
    {
        foreach (var corpus in corpora)
        {
            var parameters = new SqliteParameter[]
            {
                new(P1, corpus.Id),
                new(P2, corpus.Name),
            };
            await db.Database.ExecuteSqlRawAsync(InsertSql, parameters);
        }
    }
}
