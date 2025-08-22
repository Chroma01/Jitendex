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

using System.Text.RegularExpressions;
using System.Xml;

namespace Jitendex.Warehouse.Jmdict.Models;

internal class DocumentMetadata
{
    public required Dictionary<string, string> TagNameToDescription
    {
        set
        {
            _tagNameToDescription = value;
            _tagDescriptionToName = value.ToDictionary(x => x.Value, x => x.Key);
        }
    }
    private Dictionary<string, string> _tagNameToDescription = null!;
    private Dictionary<string, string> _tagDescriptionToName = null!;

    private readonly Dictionary<(string, string), object> TagCache = [];
    private readonly Dictionary<CorpusId, Corpus> CorpusCache = [];

    public T GetTagByName<T>(string name) where T : ITag
    {
        var key = (typeof(T).Name, name);
        if (TagCache.TryGetValue(key, out object? tag))
            return (T)tag;
        var description = _tagNameToDescription[name];
        var newTag = (T)T.New(name, description);
        TagCache.Add(key, newTag);
        return newTag;
    }

    public T GetTagByDescription<T>(string description) where T : ITag
    {
        var name = _tagDescriptionToName[description];
        return GetTagByName<T>(name);
    }

    public Corpus GetCorpus(CorpusId id)
    {
        if (CorpusCache.TryGetValue(id, out Corpus? corpus))
            return corpus;
        var newCorpus = new Corpus { Id = id };
        CorpusCache.Add(id, newCorpus);
        return newCorpus;
    }
}

internal static partial class DocumentMetadataReader
{
    public async static Task<DocumentMetadata> GetDocumentMetadataAsync(this XmlReader reader)
    {
        var dtd = await reader.GetValueAsync();
        var documentMetadata = new DocumentMetadata
        {
            TagNameToDescription = ParseDtdEntities(dtd)
                .Union(ExtraEntities())
                .ToDictionary()
        };
        return documentMetadata;
    }

    [GeneratedRegex(@"<!ENTITY\s+(.*?)\s+""(.*?)"">", RegexOptions.None)]
    private static partial Regex DtdEntityRegex();

    private static Dictionary<string, string> ParseDtdEntities(string dtd)
    {
        var nameToDescription = new Dictionary<string, string>();
        foreach (Match match in DtdEntityRegex().Matches(dtd))
        {
            var name = match.Groups[1].Value;
            var description = match.Groups[2].Value;
            try
            {
                nameToDescription.Add(name, description);
            }
            catch (ArgumentException)
            {
                if (nameToDescription[name] == description)
                {
                    // Ignore repeated definitions that are exact duplicates.
                }
                else
                {
                    throw;
                }
            }
        }
        return nameToDescription;
    }

    /// <summary>
    /// Entities that are not named explicitly in the DTD.
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, string> ExtraEntities()
    {
        var nameToDescription = new Dictionary<string, string>();
        PriorityTag.AddEntities(nameToDescription);
        GlossType.AddEntities(nameToDescription);
        CrossReferenceType.AddEntities(nameToDescription);
        return nameToDescription;
    }
}
