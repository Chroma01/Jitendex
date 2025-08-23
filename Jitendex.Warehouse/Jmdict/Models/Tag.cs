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

namespace Jitendex.Warehouse.Jmdict.Models;

internal interface ITag
{
    string Name { get; set; }
    string Description { get; set; }

    static Dictionary<string, string> DescriptionToName { get; set; } = [];

    private static readonly Dictionary<(string, string), object> Cache = [];

    static T FindByDescription<T>(string desc) where T : ITag
    {
        var key = (typeof(T).Name, desc);
        if (Cache.TryGetValue(key, out object? tag))
            return (T)tag;
        var name = DescriptionToName[desc];
        var newTag = (T)T.New(name, desc);
        Cache.Add(key, newTag);
        return newTag;
    }

    abstract static ITag New(string name, string description);
}

public class ReadingInfoTag : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    static ITag ITag.New(string name, string description)
        => new ReadingInfoTag { Name = name, Description = description };
}

public class KanjiFormInfoTag : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    static ITag ITag.New(string name, string description)
        => new KanjiFormInfoTag { Name = name, Description = description };
}

public class PartOfSpeechTag : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    static ITag ITag.New(string name, string description)
        => new PartOfSpeechTag { Name = name, Description = description };
}

public class FieldTag : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    static ITag ITag.New(string name, string description)
        => new FieldTag { Name = name, Description = description };
}

public class MiscTag : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    static ITag ITag.New(string name, string description)
        => new MiscTag { Name = name, Description = description };
}

public class DialectTag : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    static ITag ITag.New(string name, string description)
        => new DialectTag { Name = name, Description = description };
}

public class PriorityTag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }
    public bool IsHighPriority { get => HighPriorityNames.Contains(Name); set { } }

    private static readonly HashSet<string> HighPriorityNames =
        ["gai1", "ichi1", "news1", "spec1", "spec2"];

    private static readonly Dictionary<string, PriorityTag> Cache = [];

    internal static PriorityTag FindByName(string name)
    {
        if (Cache.TryGetValue(name, out PriorityTag? tag))
            return tag;
        string? description;
        if (NameToDescription.TryGetValue(name, out string? value))
        {
            description = value;
        }
        else
        {
            // TODO: Log and warn.
            description = string.Empty;
        }
        var newTag = new PriorityTag { Name = name, Description = description };
        Cache.Add(name, newTag);
        return newTag;
    }

    private static readonly Dictionary<string, string> NameToDescription = ((Func<Dictionary<string, string>>)(() =>
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
    })).Invoke();
}

public class GlossType
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    private static readonly Dictionary<string, GlossType> Cache = [];

    internal static GlossType FindByName(string name)
    {
        if (Cache.TryGetValue(name, out GlossType? tag))
            return tag;
        string? description;
        if (NameToDescription.TryGetValue(name, out string? value))
        {
            description = value;
        }
        else
        {
            // TODO: Log and warn.
            description = string.Empty;
        }
        var newTag = new GlossType { Name = name, Description = description };
        Cache.Add(name, newTag);
        return newTag;
    }

    private static readonly Dictionary<string, string> NameToDescription = new()
    {
        ["tm"] = "trademark",
        ["lit"] = "literal",
        ["fig"] = "figurative",
        ["expl"] = "explanation",
    };
}

public class CrossReferenceType
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    private static readonly Dictionary<string, CrossReferenceType> Cache = [];

    internal static CrossReferenceType FindByName(string name)
    {
        if (Cache.TryGetValue(name, out CrossReferenceType? tag))
            return tag;
        string? description;
        if (NameToDescription.TryGetValue(name, out string? value))
        {
            description = value;
        }
        else
        {
            // TODO: Log and warn.
            description = string.Empty;
        }
        var newTag = new CrossReferenceType { Name = name, Description = description };
        Cache.Add(name, newTag);
        return newTag;
    }

    private static readonly Dictionary<string, string> NameToDescription = new()
    {
        ["xref"] = "cross-reference",
        ["ant"] = "antonym",
    };
}
