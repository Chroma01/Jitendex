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

using Microsoft.EntityFrameworkCore;
using Jitendex.Chise.Readers;
using Jitendex.Chise.Database;
using Jitendex.SQLite;

namespace Jitendex.Chise;

internal static class DatabaseInitializer
{
    public static async Task WriteAsync(IdsCollector collector)
    {
        await using var db = new Context();

        // Delete and recreate the database file.
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        // For faster importing, write data to memory rather than to the disk.
        await db.Database.ExecuteSqlRawAsync(Pragmas.FastNewDatabase);

        // Using a transaction decreases the runtime by 10 seconds.
        // Using multiple smaller transactions doesn't seem to improve upon that.
        await using var transaction = await db.Database.BeginTransactionAsync();

        // Wait until all data is imported before checking foreing key constraints.
        await db.Database.ExecuteSqlRawAsync("PRAGMA defer_foreign_keys = ON;");

        // Begin inserting data.
        await db.InsertCodepointsAsync(collector.Codepoints.Values);
        await db.InsertUnicodeCharactersAsync(collector.UnicodeCharacters.Values);
        await db.InsertSequencesAsync(collector.Sequences.Values);
        await db.InsertComponentsAsync(collector.Components.Values);
        await db.InsertComponentSequencesAsync(collector.Components.Values);
        await db.InsertComponentPositionsAsync(collector.ComponentPositions.Values);

        await transaction.CommitAsync();

        // Write database to the disk.
        await db.SaveChangesAsync();

        // Rebuild the database compactly.
        await db.Database.ExecuteSqlRawAsync("VACUUM;");
    }
}
