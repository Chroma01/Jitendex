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

[Table(nameof(TokenizedSentence))]
[PrimaryKey(nameof(JapaneseSequenceId), nameof(Id))]
public sealed class TokenizedSentence
{
    public required int JapaneseSequenceId { get; init; }
    public required int Id { get; init; }
    public required int EnglishSequenceId { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(JapaneseSequenceId))]
    public JapaneseSequence JapaneseSentence { get; init; } = null!;

    [JsonIgnore]
    [ForeignKey(nameof(EnglishSequenceId))]
    public EnglishSequence EnglishSentence { get; set; } = null!;

    [InverseProperty(nameof(Token.Index))]
    public List<Token> Tokens { get; init; } = [];
}
