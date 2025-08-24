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

public class Language : IKeyword
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

    internal static readonly Dictionary<string, string> NameToDescription = ((Func<Dictionary<string, string>>)(() =>
    {
        var nameToDescription = new Dictionary<string, string>();
        foreach (var i in Enumerable.Range(1, 2))
        {
            nameToDescription.Add($"news{i}", $"Ranking in wordfreq file, {i} of 2");
            nameToDescription.Add($"ichi{i}", $"Ranking from \"Ichimango goi bunruishuu\", {i} of 2");
            nameToDescription.Add($"spec{i}", $"Ranking assigned by JMdict editors, {i} of 2");
            nameToDescription.Add($"gai{i}", $"Common loanwords based on wordfreq file, {i} of 2");
        }
        foreach (var i in Enumerable.Range(1, 48))
        {
            nameToDescription.Add($"nf{i:D2}", $"Ranking in wordfreq file, {i} of 48");
        }
        return nameToDescription;
    }))();
}
