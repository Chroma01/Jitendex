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
using Jitendex.JMdict.Data.EntryElements;
using Jitendex.JMdict.Models;

namespace Jitendex.JMdict.Data;

internal static class EntryData
{
    // Column names
    private const string C1 = nameof(Entry.Id);
    private const string C2 = nameof(Entry.CorpusId);
    private const string C3 = nameof(Entry.IsCorrupt);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(Entry)}"
        ("{C1}", "{C2}", "{C3}") VALUES
        ( {P1} ,  {P2} ,  {P3} );
        """;

    public static async Task InsertEntries(this JmdictContext db, List<Entry> entries)
    {
        foreach (var entry in entries)
        {
            var parameters = new SqliteParameter[]
            {
                new(P1, entry.Id),
                new(P2, entry.CorpusId),
                new(P3, entry.IsCorrupt),
            };
            await db.Database.ExecuteSqlRawAsync(InsertSql, parameters);
            await db.InsertReadings(entry.Readings);
            await db.InsertKanjiForms(entry.KanjiForms);
            await db.InsertSenses(entry.Senses);
        }
    }
}
