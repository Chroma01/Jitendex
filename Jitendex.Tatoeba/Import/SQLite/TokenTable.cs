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
using Jitendex.Tatoeba.Entities;
using Jitendex.Tatoeba.Import.Models;

namespace Jitendex.Tatoeba.Import.SQLite;

internal sealed class TokenTable : Table<TokenElement>
{
    protected override string Name => nameof(Token);

    protected override IReadOnlyList<string> ColumnNames =>
    [
        nameof(Token.SentenceId),
        nameof(Token.SegmentationIndex),
        nameof(Token.Index),
        nameof(Token.Headword),
        nameof(Token.Reading),
        nameof(Token.JmdictEntryId),
        nameof(Token.SenseNumber),
        nameof(Token.SentenceForm),
        nameof(Token.IsPriority),
    ];

    protected override IReadOnlyList<string> KeyColNames =>
    [
        nameof(Token.SentenceId),
        nameof(Token.SegmentationIndex),
        nameof(Token.Index),
    ];

    protected override SqliteParameter[] Parameters(TokenElement token) =>
    [
        new("@0", token.TatoebaId),
        new("@1", token.SegmentationIndex),
        new("@2", token.Index),
        new("@3", token.Headword),
        new("@4", token.Reading.Nullable()),
        new("@5", token.JmdictEntryId.Nullable()),
        new("@6", token.SenseNumber.Nullable()),
        new("@7", token.SentenceForm.Nullable()),
        new("@8", token.IsPriority),
    ];
}
