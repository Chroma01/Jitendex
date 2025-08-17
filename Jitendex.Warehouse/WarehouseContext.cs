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
