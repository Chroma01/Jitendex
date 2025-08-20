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
    public required string Name { get; set; }
    public required Dictionary<string, string> TagIdToDescription
    {
        set
        {
            _tagIdToDescription = value;
            _tagDescriptionToId = value.ToDictionary(x => x.Value, x => x.Key);
        }
    }
    private Dictionary<string, string> _tagIdToDescription = null!;
    private Dictionary<string, string> _tagDescriptionToId = null!;
    private readonly Dictionary<(string, string), object> Tags = [];

    public T GetTagById<T>(string id) where T : ITag
    {
        var key = (typeof(T).Name, id);
        if (Tags.TryGetValue(key, out object? tag))
        {
            return (T)tag;
        }
        else
        {
            var description = _tagIdToDescription[id];
            var newTag = (T)T.New(id, description);
            Tags[key] = newTag;
            return newTag;
        }
    }

    public T GetTagByDescription<T>(string description) where T : ITag
    {
        var id = _tagDescriptionToId[description];
        return GetTagById<T>(id);
    }
}

internal static partial class DocumentMetadataReader
{
    public async static Task<DocumentMetadata> GetDocumentMetadataAsync(this XmlReader reader)
    {
        var dtd = await reader.GetValueAsync();
        var documentMetadata = new DocumentMetadata
        {
            Name = reader.Name,
            TagIdToDescription = ParseDtdEntities(dtd),
        };
        return documentMetadata;
    }

    [GeneratedRegex(@"<!ENTITY\s+(.*?)\s+""(.*?)"">", RegexOptions.None)]
    private static partial Regex DtdEntityRegex();

    private static Dictionary<string, string> ParseDtdEntities(string dtd)
    {
        var idToDescription = new Dictionary<string, string>();
        foreach (Match match in DtdEntityRegex().Matches(dtd))
        {
            var id = match.Groups[1].Value;
            var description = match.Groups[2].Value;
            try
            {
                idToDescription.Add(id, description);
            }
            catch (ArgumentException)
            {
                if (idToDescription[id] == description)
                {
                    // Ignore repeated definitions that are exact duplicates.
                }
                else
                {
                    throw;
                }
            }
        }
        AddPriorityEntities(idToDescription);
        return idToDescription;
    }

    private static void AddPriorityEntities(Dictionary<string, string> idToDescription)
    {
        foreach (var i in Enumerable.Range(1, 2))
        {
            idToDescription[$"news{i}"] = $"Ranking in wordfreq file, {i} of 2";
            idToDescription[$"ichi{i}"] = $"Ranking from \"Ichimango goi bunruishuu\", {i} of 2";
            idToDescription[$"spec{i}"] = $"Ranking assigned by JMdict editors, {i} of 2";
            idToDescription[$"gai{i}"] = $"Common loanwords based on wordfreq file, {i} of 2";
        }
        foreach (var i in Enumerable.Range(1, 48))
        {
            idToDescription[$"nf{i:D2}"] = $"Ranking in wordfreq file, {i} of 48";
        }
    }
}
