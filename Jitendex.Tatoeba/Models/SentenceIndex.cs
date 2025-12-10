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

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Tatoeba.Models;

[Table(nameof(SentenceIndex))]
[PrimaryKey(nameof(SentenceId), nameof(Order))]
public sealed class SentenceIndex
{
    public required int SentenceId { get; init; }
    public required int Order { get; init; }
    public required int MeaningId { get; init; }

    [JsonIgnore]
    [ForeignKey(nameof(SentenceId))]
    public required JapaneseSentence Sentence { get; init; }

    [JsonIgnore]
    [ForeignKey(nameof(MeaningId))]
    public required EnglishSentence Meaning { get; init; }

    [InverseProperty(nameof(IndexElement.Index))]
    public List<IndexElement> Elements { get; init; } = [];

    public override bool Equals(object? obj)
        => obj is SentenceIndex index
        && SentenceId == index.SentenceId
        && Order == index.Order
        && MeaningId == index.MeaningId
        && Enumerable.SequenceEqual(Elements, index.Elements);

    public override int GetHashCode()
        => Elements.Aggregate
        (
            seed: HashCode.Combine(SentenceId, Order, MeaningId),
            func: static (hash, element) => HashCode.Combine(hash, element)
        );

    internal (int, int) Key => (SentenceId, Order);
}
