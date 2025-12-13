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
using Jitendex.Kanjidic2.Models;
using Jitendex.SQLite;

namespace Jitendex.Kanjidic2.Database;

internal sealed class EntryTable : Table<Entry>
{
    protected override string Name => nameof(Entry);

    protected override IReadOnlyList<string> ColumnNames =>
    [
        nameof(Entry.UnicodeScalarValue),
        nameof(Entry.Grade),
        nameof(Entry.Frequency),
        nameof(Entry.JlptLevel),
        nameof(Entry.IsKokuji),
        nameof(Entry.IsGhost),
        nameof(Entry.IsCorrupt),
    ];

    protected override IReadOnlyList<string> KeyColNames =>
    [
        nameof(Entry.UnicodeScalarValue)
    ];

    protected override SqliteParameter[] Parameters(Entry entry) =>
    [
        new("@0", entry.UnicodeScalarValue),
        new("@1", entry.Grade.Nullable()),
        new("@2", entry.Frequency.Nullable()),
        new("@3", entry.JlptLevel.Nullable()),
        new("@4", entry.IsKokuji),
        new("@5", entry.IsGhost),
        new("@6", entry.IsCorrupt),
    ];
}
