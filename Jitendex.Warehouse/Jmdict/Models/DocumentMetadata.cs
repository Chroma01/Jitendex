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
    public required Dictionary<string, string> EntityDescriptionToName { get; set; }
    private readonly Dictionary<string, string> PriorityTagNameToDesctiption = PriorityTag.Entities();
    private readonly Dictionary<string, string> CrossReferenceTypeNameToDesctiption = CrossReferenceType.Entities();
    private readonly Dictionary<string, string> GlossTypeNameToDesctiption = GlossType.Entities();

    private readonly Dictionary<(string, string), object> ITagCache = [];
    private readonly Dictionary<string, PriorityTag> PriorityTagCache = [];
    private readonly Dictionary<string, CrossReferenceType> CrossReferenceTypeCache = [];
    private readonly Dictionary<string, GlossType> GlossTypeCache = [];
    private readonly Dictionary<CorpusId, Corpus> CorpusCache = [];

    public T GetTagByDescription<T>(string desc) where T : ITag
    {
        var key = (typeof(T).Name, desc);
        if (ITagCache.TryGetValue(key, out object? tag))
            return (T)tag;
        var name = EntityDescriptionToName[desc];
        var newTag = (T)T.New(name, desc);
        ITagCache.Add(key, newTag);
        return newTag;
    }

    public PriorityTag GetPriorityTag(string name)
    {
        if (PriorityTagCache.TryGetValue(name, out PriorityTag? tag))
            return tag;
        var description = PriorityTagNameToDesctiption[name];
        var newTag = new PriorityTag { Name = name, Description = description };
        PriorityTagCache.Add(name, newTag);
        return newTag;
    }

    public CrossReferenceType GetCrossReferenceType(string name)
    {
        if (CrossReferenceTypeCache.TryGetValue(name, out CrossReferenceType? type))
            return type;
        var description = CrossReferenceTypeNameToDesctiption[name];
        var newType = new CrossReferenceType { Name = name, Description = description };
        CrossReferenceTypeCache.Add(name, newType);
        return newType;
    }

    public GlossType GetGlossType(string name)
    {
        if (GlossTypeCache.TryGetValue(name, out GlossType? type))
            return type;
        var description = GlossTypeNameToDesctiption[name];
        var newType = new GlossType { Name = name, Description = description };
        GlossTypeCache.Add(name, newType);
        return newType;
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
            EntityDescriptionToName = ParseDtdEntities(dtd),
        };
        return documentMetadata;
    }

    [GeneratedRegex(@"<!ENTITY\s+(.*?)\s+""(.*?)"">", RegexOptions.None)]
    private static partial Regex DtdEntityRegex();

    private static Dictionary<string, string> ParseDtdEntities(string dtd)
    {
        var descriptionToName = new Dictionary<string, string>();
        foreach (Match match in DtdEntityRegex().Matches(dtd))
        {
            var name = match.Groups[1].Value;
            var description = match.Groups[2].Value;
            try
            {
                descriptionToName.Add(description, name);
            }
            catch (ArgumentException)
            {
                if (descriptionToName[description] == name)
                {
                    // Ignore repeated definitions that are exact duplicates.
                }
                else
                {
                    throw;
                }
            }
        }
        return descriptionToName;
    }
}
