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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jitendex.Tatoeba.Models;

[Table(nameof(EnglishSentence))]
public sealed class EnglishSentence
{
    [Key]
    public required int SequenceId { get; init; }
    public required string Text { get; init; }

    [ForeignKey(nameof(SequenceId))]
    public required Sequence Sequence { get; init; }

    [InverseProperty(nameof(SentenceIndex.Meaning))]
    public List<SentenceIndex> Indices { get; init; } = [];
}
