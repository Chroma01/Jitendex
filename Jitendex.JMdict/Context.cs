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
using Microsoft.EntityFrameworkCore.Storage;
using Jitendex.JMdict.Models;

namespace Jitendex.JMdict;

public class Context : DbContext
{
    public DbSet<Entry> Entries { get; set; } = null!;
    public DbSet<Corpus> Corpora { get; set; } = null!;

    public DbSet<PriorityTag> PriorityTags { get; set; } = null!;
    public DbSet<ReadingInfoTag> ReadingInfoTags { get; set; } = null!;
    public DbSet<KanjiFormInfoTag> KanjiFormInfoTags { get; set; } = null!;

    public DbSet<PartOfSpeechTag> PartOfSpeechTags { get; set; } = null!;
    public DbSet<FieldTag> FieldTags { get; set; } = null!;
    public DbSet<MiscTag> MiscTags { get; set; } = null!;
    public DbSet<DialectTag> DialectTags { get; set; } = null!;

    public DbSet<GlossType> GlossTypes { get; set; } = null!;
    public DbSet<CrossReferenceType> CrossReferenceTypes { get; set; } = null!;
    public DbSet<LanguageSourceType> LanguageSourceTypes { get; set; } = null!;
    public DbSet<Language> Languages { get; set; } = null!;

    public DbSet<ExampleSourceType> ExampleSourceTypes { get; set; } = null!;
    public DbSet<ExampleSource> ExampleSources { get; set; } = null!;

    public string DbPath { get; }

    public Context()
    {
        var dbFolder = Path.Join
        (
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Jitendex"
        );
        Directory.CreateDirectory(dbFolder);
        DbPath = Path.Join(dbFolder, "jmdict.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options
        .UseSqlite($"Data Source={DbPath}")
        .ReplaceService<IRelationalCommandBuilderFactory, JmdictRelationalCommandBuilderFactory>();
}

internal class JmdictRelationalCommandBuilderFactory : RelationalCommandBuilderFactory
{
    public JmdictRelationalCommandBuilderFactory(RelationalCommandBuilderDependencies dependencies) : base(dependencies) { }
    public override IRelationalCommandBuilder Create() => new JmdictRelationalCommandBuilder(Dependencies);
}

internal class JmdictRelationalCommandBuilder : RelationalCommandBuilder
{
    public JmdictRelationalCommandBuilder(RelationalCommandBuilderDependencies dependencies) : base(dependencies) { }
    public override IRelationalCommand Build() => new RelationalCommand
    (
        Dependencies,
        base.ToString().WithoutRowId(),
        Parameters
    );
}

internal static class CommandTextExtensions
{
    public static string WithoutRowId(this string commandText)
    {
        if (!commandText.StartsWith("CREATE TABLE", StringComparison.Ordinal))
        {
            return commandText;
        }

        if (commandText.Contains("AUTOINCREMENT", StringComparison.Ordinal))
        {
            return commandText;
        }

        int endCreateTableIndex = commandText.IndexOf(");", StringComparison.Ordinal);

        if (endCreateTableIndex < 0)
        {
            return commandText;
        }

        // Insert " WITHOUT ROWID" between the ")" and the ";"
        return commandText[..(endCreateTableIndex + 1)]
            + " WITHOUT ROWID"
            + commandText[(endCreateTableIndex + 1)..];
    }
}
