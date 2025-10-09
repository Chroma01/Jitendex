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
using Microsoft.EntityFrameworkCore;
using Jitendex.JMdict.Models.EntryElements.SenseElements;

namespace Jitendex.JMdict.Models.EntryElements;

[PrimaryKey(nameof(EntryId), nameof(Order))]
public class Sense
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public string? Note { get; set; }

    public List<KanjiFormRestriction> KanjiFormRestrictions { get; set; } = [];
    public List<ReadingRestriction> ReadingRestrictions { get; set; } = [];

    public List<PartOfSpeech> PartsOfSpeech { get; set; } = [];
    public List<Field> Fields { get; set; } = [];
    public List<Misc> Miscs { get; set; } = [];
    public List<Dialect> Dialects { get; set; } = [];

    public List<Gloss> Glosses { get; set; } = [];
    public List<LanguageSource> LanguageSources { get; set; } = [];
    public List<Example> Examples { get; set; } = [];

    [NotMapped]
    internal List<RawCrossReference> RawCrossReferences { get; set; } = [];

    [InverseProperty(nameof(CrossReference.Sense))]
    public List<CrossReference> CrossReferences { get; set; } = [];

    [InverseProperty(nameof(CrossReference.RefSense))]
    public List<CrossReference> ReverseCrossReferences { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public required Entry Entry { get; set; }

    internal const string XmlTagName = "sense";
    internal const string Note_XmlTagName = "s_inf";
}
