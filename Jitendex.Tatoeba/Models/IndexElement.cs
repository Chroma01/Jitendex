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

[Table(nameof(IndexElement))]
[PrimaryKey(nameof(SentenceId), nameof(MeaningId), nameof(IndexOrder), nameof(Order))]
public class IndexElement
{
    public required int SentenceId { get; init; }
    public required int MeaningId { get; init; }
    public required int IndexOrder { get; init; }
    public required int Order { get; init; }

    public required string Headword { get; init; }
    public required string? Reading { get; init; }
    public required int? EntryId { get; init; }
    public required int? SenseNumber { get; init; }
    public required string? SentenceForm { get; init; }
    public required bool IsPriority { get; init; }

    [ForeignKey($"{nameof(SentenceId)}, {nameof(MeaningId)}, {nameof(IndexOrder)}")]
    public required SentenceIndex Index { get; init; }

    public override string ToString() =>
        $"{SentenceId}\t{MeaningId}\t{IndexOrder}\t{Order}\t{Headword}"
        + '\t' + (Reading is not null ? Reading : "")
        + '\t' + (EntryId is not null ? EntryId : "")
        + '\t' + (SenseNumber is not null ? $"{SenseNumber:D2}" : "")
        + '\t' + (SentenceForm is not null ? SentenceForm : "")
        + '\t' + (IsPriority ? "1" : "0");
}
