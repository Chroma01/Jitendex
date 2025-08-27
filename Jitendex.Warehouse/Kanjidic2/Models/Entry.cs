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
using Jitendex.Warehouse.Kanjidic2.Models.Groups;
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Models;

[Table("Entry")]
public class Entry : ICorruptable
{
    [Key]
    public required string Character { get; set; }

    // Codepoint Group
    public virtual List<Codepoint> Codepoints { get; set; } = [];

    // Dictionary Group
    public virtual List<Dictionary> Dictionaries { get; set; } = [];

    // Misc Group
    public int? Grade { get; set; }
    public int? Frequency { get; set; }
    public int? JlptLevel { get; set; }
    public virtual List<StrokeCount> StrokeCounts { get; set; } = [];
    public virtual List<Variant> Variants { get; set; } = [];
    public virtual List<RadicalName> RadicalNames { get; set; } = [];

    // Query Code Group
    public virtual List<QueryCode> QueryCodes { get; set; } = [];

    // Radical Group
    public virtual List<Radical> Radicals { get; set; } = [];

    // Reading Meaning Group
    public required bool IsKokuji { get; set; }
    public virtual List<Reading> Readings { get; set; } = [];
    public virtual List<Meaning> Meanings { get; set; } = [];
    public virtual List<Nanori> Nanoris { get; set; } = [];

    [NotMapped]
    internal CodepointGroup? CodepointGroup = null;
    [NotMapped]
    internal DictionaryGroup? DictionaryGroup = null;
    [NotMapped]
    internal MiscGroup? MiscGroup = null;
    [NotMapped]
    internal QueryCodeGroup? QueryCodeGroup = null;
    [NotMapped]
    internal RadicalGroup? RadicalGroup = null;
    [NotMapped]
    internal ReadingMeaningGroup? ReadingMeaningGroup = null;

    public bool IsCorrupt { get; set; }

    internal const string XmlTagName = "character";
}
