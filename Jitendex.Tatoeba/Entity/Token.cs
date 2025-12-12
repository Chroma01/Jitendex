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

namespace Jitendex.Tatoeba.Entity;

[Table(nameof(Token))]
[PrimaryKey(nameof(SequenceId), nameof(SentenceId), nameof(Id))]
public sealed class Token
{
    public required int SequenceId { get; init; }
    public required int SentenceId { get; init; }
    public required int Id { get; init; }

    public required string Headword { get; set; }
    public required string? Reading { get; set; }
    public required int? EntryId { get; set; }
    public required int? SenseNumber { get; set; }
    public required string? SentenceForm { get; set; }
    public required bool IsPriority { get; set; }

    [JsonIgnore]
    [ForeignKey($"{nameof(SequenceId)}, {nameof(SentenceId)}")]
    public TokenizedSentence Index { get; init; } = null!;

    public override string ToString() =>
        $"{SequenceId}\t{SentenceId}\t{Id}\t{Headword}"
        + '\t' + (Reading ?? "")
        + '\t' + (EntryId.HasValue ? EntryId : "")
        + '\t' + (SenseNumber.HasValue ? $"{SenseNumber:D2}" : "")
        + '\t' + (SentenceForm ?? "")
        + '\t' + (IsPriority ? "1" : "0");
}
