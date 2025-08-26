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
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Models;

public class Entry
{
    [Key]
    public required string Character { get; set; }
    public List<Codepoint> Codepoints { get; set; } = [];
    public List<Radical> Radicals { get; set; } = [];
    public List<Reading> Readings { get; set; } = [];
    public List<Meaning> Meanings { get; set; } = [];
    public List<Nanori> Nanoris { get; set; } = [];
    public required bool IsKokuji { get; set; }
    public int? Grade { get; set; }
    public int? Frequency { get; set; }
    public int? JlptLevel { get; set; }
    public List<StrokeCount> StrokeCounts { get; set; } = [];
    public List<Variant> Variants { get; set; } = [];
    public List<RadicalName> RadicalNames { get; set; } = [];
    public List<Dictionary> Dictionaries { get; set; } = [];
    public List<QueryCode> QueryCodes { get; set; } = [];

    internal const string XmlTagName = "character";
}
