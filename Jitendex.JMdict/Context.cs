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

public class JmdictContext : DbContext
{
    public DbSet<Entry> Entries { get; set; } = null!;

    public string DbPath { get; }

    public JmdictContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbFolder = Path.Join(path, "Jitendex");
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
    private const string _beginCreateTable = "CREATE TABLE";
    private const string _endCreateTable = ");";
    private const string _withoutRowId = ") WITHOUT ROWID;";
    private const string _autoincrement = "AUTOINCREMENT";

    public static string WithoutRowId(this string commandText)
    {
        if (!commandText.StartsWith(_beginCreateTable, StringComparison.Ordinal))
        {
            return commandText;
        }

        if (commandText.Contains(_autoincrement, StringComparison.Ordinal))
        {
            return commandText;
        }

        int endCreateTableIndex = commandText.IndexOf(_endCreateTable, StringComparison.Ordinal);

        if (endCreateTableIndex < 0)
        {
            return commandText;
        }

        return commandText[..endCreateTableIndex]
            + _withoutRowId
            + commandText[(endCreateTableIndex + _endCreateTable.Length)..];
    }
}
