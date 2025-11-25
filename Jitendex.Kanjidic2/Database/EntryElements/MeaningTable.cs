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
using Jitendex.Kanjidic2.Models.EntryElements;
using Jitendex.SQLite;

namespace Jitendex.Kanjidic2.Database.EntryElements;

internal sealed class MeaningTable : Table<Meaning>
{
    protected override string Name => nameof(Meaning);

    protected override IReadOnlyList<string> ColumnNames =>
    [
        nameof(Meaning.UnicodeScalarValue),
        nameof(Meaning.Order),
        nameof(Meaning.Text),
    ];

    protected override SqliteParameter[] Parameters(Meaning meaning) =>
    [
        new("@0", meaning.UnicodeScalarValue),
        new("@1", meaning.Order),
        new("@2", meaning.Text),
    ];
}
