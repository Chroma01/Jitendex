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
using Jitendex.Tatoeba.Models;
using Jitendex.SQLite;

namespace Jitendex.Tatoeba.SQLite;

internal sealed class IndexElementTable : Table<IndexElement>
{
    protected override string Name => nameof(IndexElement);

    protected override IReadOnlyList<string> ColumnNames =>
    [
        nameof(IndexElement.SentenceId),
        nameof(IndexElement.IndexOrder),
        nameof(IndexElement.Order),
        nameof(IndexElement.Headword),
        nameof(IndexElement.Reading),
        nameof(IndexElement.EntryId),
        nameof(IndexElement.SenseNumber),
        nameof(IndexElement.SentenceForm),
        nameof(IndexElement.IsPriority),
    ];

    protected override SqliteParameter[] Parameters(IndexElement element) =>
    [
        new("@0", element.SentenceId),
        new("@1", element.IndexOrder),
        new("@2", element.Order),
        new("@3", element.Headword),
        new("@4", element.Reading.Nullable()),
        new("@5", element.EntryId.Nullable()),
        new("@6", element.SenseNumber.Nullable()),
        new("@7", element.SentenceForm.Nullable()),
        new("@8", element.IsPriority),
    ];
}
