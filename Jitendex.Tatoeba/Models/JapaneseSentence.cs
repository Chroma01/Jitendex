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
using System.Text.Json.Serialization;

namespace Jitendex.Tatoeba.Models;

[Table(nameof(JapaneseSentence))]
public sealed class JapaneseSentence
{
    [Key]
    public required int SequenceId { get; init; }
    public required string Text { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(SequenceId))]
    public Sequence Sequence { get; init; } = null!;

    [InverseProperty(nameof(SentenceIndex.Sentence))]
    public List<SentenceIndex> Indices { get; init; } = [];

    public override bool Equals(object? obj)
        => obj is JapaneseSentence sentence
        && SequenceId == sentence.SequenceId
        && string.Equals(Text, sentence.Text, StringComparison.Ordinal)
        && Enumerable.SequenceEqual(Indices, sentence.Indices);

    public override int GetHashCode()
        => Indices.Aggregate
        (
            seed: HashCode.Combine(SequenceId, Text),
            func: static (hash, index) => HashCode.Combine(hash, index)
        );
}
