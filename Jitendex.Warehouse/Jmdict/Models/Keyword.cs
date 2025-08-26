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

using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;

namespace Jitendex.Warehouse.Jmdict.Models;

internal interface IKeyword
{
    string Name { get; set; }
    string Description { get; set; }
}

public class ReadingInfoTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class KanjiFormInfoTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class PartOfSpeechTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class FieldTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class MiscTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class DialectTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class GlossType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    internal static readonly Dictionary<string, string> NameToDescription = new()
    {
        ["tm"] = "trademark",
        ["lit"] = "literal",
        ["fig"] = "figurative",
        ["expl"] = "explanation",
    };
}

public class CrossReferenceType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    internal static readonly Dictionary<string, string> NameToDescription = new()
    {
        ["xref"] = "cross-reference",
        ["ant"] = "antonym",
    };
}

public class LanguageSourceType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    internal static readonly Dictionary<string, string> NameToDescription = new()
    {
        ["full"] = "Full description of the source word or phrase of the loanword",
        ["part"] = "Partial description of the source word or phrase of the loanword",
    };
}

public class ExampleSourceType : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    internal static readonly Dictionary<string, string> NameToDescription = new()
    {
        ["tat"] = "tatoeba.org",
    };
}

public class PriorityTag : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsHighPriority { get => HighPriorityNames.Contains(Name); set { } }

    private static readonly FrozenSet<string> HighPriorityNames =
        ["gai1", "ichi1", "news1", "spec1", "spec2"];

    internal static readonly Dictionary<string, string> NameToDescription =
        Enumerable.Range(1, 2).SelectMany(i => new KeyValuePair<string, string>[] {
            new($"news{i}", $"Ranking in wordfreq file, {i} of 2"),
            new($"ichi{i}", $"Ranking from \"Ichimango goi bunruishuu\", {i} of 2"),
            new($"spec{i}", $"Ranking assigned by JMdict editors, {i} of 2"),
            new($"gai{i}",  $"Common loanwords based on wordfreq file, {i} of 2"),
        }).Concat(
        Enumerable.Range(1, 48).Select(i => new KeyValuePair<string, string>
            ($"nf{i:D2}", $"Ranking in wordfreq file, {i} of 48")
        )).ToDictionary();
}

public class Language : IKeyword
{
    [Key]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    internal static readonly Dictionary<string, string> NameToDescription = new()
    {
        ["afr"] = "Afrikaans",
        ["ain"] = "Ainu",
        ["alg"] = "Algonquian",
        ["amh"] = "Amharic",
        ["ara"] = "Arabic",
        ["arn"] = "Mapudungun",
        ["bnt"] = "Bantu",
        ["bre"] = "Breton",
        ["bul"] = "Bulgarian",
        ["bur"] = "Burmese",
        ["chi"] = "Chinese",
        ["chn"] = "Chinook Jargon",
        ["cze"] = "Czech",
        ["dan"] = "Danish",
        ["dut"] = "Dutch",
        ["eng"] = "English",
        ["epo"] = "Esperanto",
        ["est"] = "Estonian",
        ["fil"] = "Filipino",
        ["fin"] = "Finnish",
        ["fre"] = "French",
        ["geo"] = "Georgian",
        ["ger"] = "German",
        ["glg"] = "Galician",
        ["grc"] = "Ancient Greek",
        ["gre"] = "Modern Greek",
        ["haw"] = "Hawaiian",
        ["heb"] = "Hebrew",
        ["hin"] = "Hindi",
        ["hun"] = "Hungarian",
        ["ice"] = "Icelandic",
        ["ind"] = "Indonesian",
        ["ita"] = "Italian",
        ["khm"] = "Khmer",
        ["kor"] = "Korean",
        ["kur"] = "Kurdish",
        ["lat"] = "Latin",
        ["lit"] = "Lithuanian",
        ["mal"] = "Malayalam",
        ["mao"] = "Maori",
        ["may"] = "Malay",
        ["mnc"] = "Manchu",
        ["mol"] = "Moldavian",
        ["mon"] = "Mongolian",
        ["nor"] = "Norwegian",
        ["per"] = "Persian",
        ["pol"] = "Polish",
        ["por"] = "Portuguese",
        ["rum"] = "Romanian",
        ["rus"] = "Russian",
        ["san"] = "Sanskrit",
        ["scr"] = "Croatian",
        ["slo"] = "Slovak",
        ["slv"] = "Slovenian",
        ["som"] = "Somali",
        ["spa"] = "Spanish",
        ["swa"] = "Swahili",
        ["swe"] = "Swedish",
        ["tah"] = "Tahitian",
        ["tam"] = "Tamil",
        ["tgl"] = "Tagalog",
        ["tha"] = "Thai",
        ["tib"] = "Tibetan",
        ["tur"] = "Turkish",
        ["ukr"] = "Ukrainian",
        ["urd"] = "Urdu",
        ["uzb"] = "Uzbek",
        ["vie"] = "Vietnamese",
        ["yid"] = "Yiddish"
    };
}
