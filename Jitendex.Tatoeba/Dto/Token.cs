/*
Copyright (c) 2025 Stephen Kraus

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

namespace Jitendex.Tatoeba.Dto;

internal sealed record Token
{
    public required int SequenceId { get; init; }
    public required int SentenceId { get; init; }
    public required int Id { get; init; }
    public required string Headword { get; init; }
    public required string? Reading { get; init; }
    public required int? EntryId { get; init; }
    public required int? SenseNumber { get; init; }
    public required string? SentenceForm { get; init; }
    public required bool IsPriority { get; init; }
    public (int, int, int) Key => (SequenceId, SentenceId, Id);
}
