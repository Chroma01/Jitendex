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

public interface ITag
{
    string Name { get; set; }
    string Description { get; set; }

    internal abstract static ITag New(string name, string description);
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

public class PriorityTag : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required bool IsHighPriority { get; set; }

    static ITag ITag.New(string name, string description)
        => new PriorityTag
        {
            Name = name,
            Description = description,
            IsHighPriority = HighPriorityNames.Contains(name),
        };

    private static readonly HashSet<string> HighPriorityNames =
        ["gai1", "ichi1", "news1", "spec1", "spec2"];

    internal static void AddEntities(Dictionary<string, string> nameToDescription)
    {
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
    }
}

public class GlossType : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    static ITag ITag.New(string name, string description)
        => new GlossType { Name = name, Description = description };

    internal static void AddEntities(Dictionary<string, string> nameToDescription)
    {
        nameToDescription.Add("tm", "trademark gloss");
        nameToDescription.Add("lit", "literal");
        nameToDescription.Add("fig", "figurative");
        nameToDescription.Add("expl", "explanation");
    }
}

public class CrossReferenceType : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    static ITag ITag.New(string name, string description)
        => new CrossReferenceType { Name = name, Description = description };

    internal static void AddEntities(Dictionary<string, string> nameToDescription)
    {
        nameToDescription.Add("xref", "cross-reference");
        nameToDescription.Add("ant", "antonym");
    }
}


public class Corpus : ITag
{
    [Key]
    public required string Name { get; set; }
    public required string Description { get; set; }

    static ITag ITag.New(string name, string description)
        => new Corpus { Name = name, Description = description };

    internal static void AddEntities(Dictionary<string, string> nameToDescription)
    {
        nameToDescription.Add(CorpusId.Unknown.ToString(), "Entries with unexpected IDs (this is bad)");
        nameToDescription.Add(CorpusId.Jmdict.ToString(), "Japanese-Multilingual Dictionary");
        nameToDescription.Add(CorpusId.Jmnedict.ToString(), "Japanese Multilingual Named Entity Dictionary");
        nameToDescription.Add(CorpusId.Metadata.ToString(), "Dictionary creation date entry");
    }

    private enum CorpusId
    {
        Unknown,
        Jmdict,
        Jmnedict,
        Metadata,
    }

    internal static string EntryIdToCorpusName(int entryId)
        => (entryId switch
        {
            < 1000000 => CorpusId.Unknown,
            < 3000000 => CorpusId.Jmdict,
            < 5000000 => CorpusId.Unknown,
            < 6000000 => CorpusId.Jmnedict,
            < 9999999 => CorpusId.Unknown,
              9999999 => CorpusId.Metadata,
                    _ => CorpusId.Unknown,
        }).ToString();
}
