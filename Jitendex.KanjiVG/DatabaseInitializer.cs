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
using Jitendex.KanjiVG.Database;
using Jitendex.KanjiVG.Models;
using Jitendex.SQLite;

namespace Jitendex.KanjiVG;

internal static class DatabaseInitializer
{
    public static async Task WriteAsync(KanjiVGDocument kanjivg)
    {
        await using var db = new Context();

        // Delete and recreate the database file.
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        // For faster importing, write data to memory rather than to the disk.
        await db.Database.ExecuteSqlRawAsync(Pragmas.FastNewDatabase);

        // Begin inserting data.
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            await db.InsertLookupsAsync<VariantType>(kanjivg.VariantTypes);
            await db.InsertLookupsAsync<ComponentGroupStyle>(kanjivg.ComponentGroupStyles);
            await db.InsertLookupsAsync<StrokeNumberGroupStyle>(kanjivg.StrokeNumberGroupStyles);
            await db.InsertEntriesAsync(kanjivg.Entries);
            await transaction.CommitAsync();
        }

        // Write database to the disk.
        await db.SaveChangesAsync();

        // Rebuild the database compactly.
        await db.Database.ExecuteSqlRawAsync("VACUUM;");
    }
}
