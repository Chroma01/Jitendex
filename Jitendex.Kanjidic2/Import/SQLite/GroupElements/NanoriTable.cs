/*
Copyright (c) 2025-2026 Stephen Kraus
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
using Jitendex.SQLite;
using Jitendex.Kanjidic2.Import.Models.GroupElements;

namespace Jitendex.Kanjidic2.Import.SQLite.GroupElements;

internal sealed class NanoriTable : Table<Nanori>
{
    protected override string Name => nameof(Nanori);

    protected override IReadOnlyList<string> ColumnNames =>
    [
        nameof(Nanori.UnicodeScalarValue),
        nameof(Nanori.GroupOrder),
        nameof(Nanori.Order),
        nameof(Nanori.Text),
    ];

    protected override IReadOnlyList<string> KeyColNames =>
    [
        nameof(Nanori.UnicodeScalarValue),
        nameof(Nanori.GroupOrder),
        nameof(Nanori.Order),
    ];

    protected override SqliteParameter[] Parameters(Nanori nanori) =>
    [
        new("@0", nanori.UnicodeScalarValue),
        new("@1", nanori.GroupOrder),
        new("@2", nanori.Order),
        new("@3", nanori.Text),
    ];
}
