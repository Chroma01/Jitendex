/*
Copyright (c) 2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms
of the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

namespace Jitendex.Tatoeba.Import.Models;

internal sealed record DocumentHeader(DateOnly Date);
internal sealed record SequenceElement(int Id, DateOnly CreatedDate);
internal sealed record EntryElement(int SequenceId);
internal sealed record EnglishSentenceElement(int EntryId, string Text);
internal sealed record JapaneseSentenceElement(int EntryId, string Text);

internal sealed record SegmentationElement(int JapaneseId, int Index, int EnglishId)
{
    public (int, int) GetKey() => (JapaneseId, Index);
}

internal sealed record TokenElement
{
    public required int TatoebaId { get; init; }
    public required int SegmentationIndex { get; init; }
    public required int Index { get; init; }
    public required string Headword { get; init; }
    public required string? Reading { get; init; }
    public required int? JmdictEntryId { get; init; }
    public required int? SenseNumber { get; init; }
    public required string? SentenceForm { get; init; }
    public required bool IsPriority { get; init; }
    public (int, int, int) GetKey() => (TatoebaId, SegmentationIndex, Index);
}
