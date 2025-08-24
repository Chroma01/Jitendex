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
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements;

[PrimaryKey(nameof(EntryId), nameof(Order))]
public class Sense
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public string? Note { get; set; }

    public virtual List<PartOfSpeech> PartsOfSpeech { get; set; } = [];
    public virtual List<Field> Fields { get; set; } = [];
    public virtual List<Misc> Miscs { get; set; } = [];
    public virtual List<Dialect> Dialects { get; set; } = [];

    public virtual List<Gloss> Glosses { get; set; } = [];
    public virtual List<LanguageSource> LanguageSources { get; set; } = [];
    public virtual List<Example> Examples { get; set; } = [];

    [InverseProperty(nameof(CrossReference.Sense))]
    public virtual List<CrossReference> CrossReferences { get; set; } = [];

    [InverseProperty(nameof(CrossReference.RefSense))]
    public virtual List<CrossReference> ReverseCrossReferences { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    [NotMapped]
    internal List<string> ReadingTextRestrictions { get; set; } = [];
    [NotMapped]
    internal List<string> KanjiFormTextRestrictions { get; set; } = [];

    internal const string XmlTagName = "sense";
}
