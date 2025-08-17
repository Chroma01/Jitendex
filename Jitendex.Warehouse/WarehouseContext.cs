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
using JmdictEntry = Jitendex.Warehouse.Jmdict.Models.Entry;
using Kanjidic2Entry = Jitendex.Warehouse.Kanjidic2.Models.Entry;

namespace Jitendex.Warehouse;

public class WarehouseContext : DbContext
{
    public DbSet<JmdictEntry> JmdictEntries { get; set; } = null!;
    public DbSet<Kanjidic2Entry> Kanjidic2Entries { get; set; } = null!;

    public string DbPath { get; }

    public WarehouseContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbFolder = Path.Join(path, "Jitendex");
        Directory.CreateDirectory(dbFolder);
        DbPath = Path.Join(dbFolder, "warehouse.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}
