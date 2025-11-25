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

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jitendex.Tatoeba.Models;

[Table(nameof(SentenceIndex))]
[PrimaryKey(nameof(SentenceId), nameof(MeaningId), nameof(Order))]
public sealed class SentenceIndex
{
    public required int SentenceId { get; init; }
    public required int MeaningId { get; init; }
    public required int Order { get; init; }

    [ForeignKey(nameof(SentenceId))]
    public required JapaneseSentence Sentence { get; init; }

    [ForeignKey(nameof(MeaningId))]
    public required EnglishSentence Meaning { get; init; }

    [InverseProperty(nameof(IndexElement.Index))]
    public List<IndexElement> Elements { get; init; } = [];

    internal (int, int, int) Key => (SentenceId, MeaningId, Order);
}
