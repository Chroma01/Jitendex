/*
Copyright (c) 2025 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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
using Jitendex.Kanjidic2.Entities.EntryElements;
using Jitendex.SQLite;

namespace Jitendex.Kanjidic2.Import.SQLite.EntryElements;

internal sealed class DictionaryTable : Table<Dictionary>
{
    protected override string Name => nameof(Dictionary);

    protected override IReadOnlyList<string> ColumnNames =>
    [
        nameof(Dictionary.UnicodeScalarValue),
        nameof(Dictionary.Order),
        nameof(Dictionary.Text),
        nameof(Dictionary.TypeName),
        nameof(Dictionary.Volume),
        nameof(Dictionary.Page),
    ];

    protected override IReadOnlyList<string> KeyColNames =>
    [
        nameof(Dictionary.UnicodeScalarValue)
    ];

    protected override SqliteParameter[] Parameters(Dictionary dictionary) =>
    [
        new("@0", dictionary.UnicodeScalarValue),
        new("@1", dictionary.Order),
        new("@2", dictionary.Text),
        new("@3", dictionary.TypeName),
        new("@4", dictionary.Volume.Nullable()),
        new("@5", dictionary.Page.Nullable()),
    ];
}
