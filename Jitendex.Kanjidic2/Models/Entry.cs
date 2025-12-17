/*
Copyright (c) 2025 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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
using System.Text;
using Jitendex.Kanjidic2.Models.Groups;
using Jitendex.Kanjidic2.Models.EntryElements;

namespace Jitendex.Kanjidic2.Models;

[Table("Entry")]
public class Entry : ICorruptable
{
    [Key]
    public required int UnicodeScalarValue { get; set; }

    // Codepoint Group
    public List<Codepoint> Codepoints { get; set; } = [];

    // Dictionary Group
    public List<Dictionary> Dictionaries { get; set; } = [];

    // Misc Group
    public int? Grade { get; set; }
    public int? Frequency { get; set; }
    public int? JlptLevel { get; set; }
    public List<StrokeCount> StrokeCounts { get; set; } = [];
    public List<Variant> Variants { get; set; } = [];
    public List<RadicalName> RadicalNames { get; set; } = [];

    // Query Code Group
    public List<QueryCode> QueryCodes { get; set; } = [];

    // Radical Group
    public List<Radical> Radicals { get; set; } = [];

    // Reading Meaning Group
    public bool IsKokuji { get; set; }
    public bool IsGhost { get; set; }
    public List<Reading> Readings { get; set; } = [];
    public List<Meaning> Meanings { get; set; } = [];
    public List<Nanori> Nanoris { get; set; } = [];

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

    public Rune ToRune() => new(UnicodeScalarValue);

    internal const string XmlTagName = "character";
    internal const string Character_XmlTagName = "literal";
}
