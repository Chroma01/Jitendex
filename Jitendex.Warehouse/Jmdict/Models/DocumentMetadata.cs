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
    public required Dictionary<string, string> EntityValueToName { get; set; }

    private Dictionary<(string, string), object> TagDescriptions { get; set; } = [];

    public T GetTagDescription<T>(string entityValue) where T : ITagDescription
    {
        var key =
        (
            tagTypeName: typeof(T).Name,
            entityName: EntityValueToName[entityValue]
        );
        if (TagDescriptions.TryGetValue(key, out object? tagDescription))
        {
            return (T)tagDescription;
        }
        else
        {
            var newTagDescription = (T)T.New(key.entityName, entityValue);
            TagDescriptions[key] = newTagDescription;
            return newTagDescription;
        }
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
            EntityValueToName = ParseEntityDeclarations(dtd),
        };
        return documentMetadata;
    }

    [GeneratedRegex(@"<!ENTITY\s+(.*?)\s+""(.*?)"">", RegexOptions.None)]
    private static partial Regex DtdRegex();

    private static Dictionary<string, string> ParseEntityDeclarations(string dtd)
    {
        var valueToName = new Dictionary<string, string>();
        foreach (Match match in DtdRegex().Matches(dtd))
        {
            var entityName = match.Groups[1].Value;
            var entityValue = match.Groups[2].Value;
            try
            {
                valueToName.Add(entityValue, entityName);
            }
            catch (ArgumentException)
            {
                // The "ik" name is reused with the same value.
                // As long as the Name/Value pair is the same,
                // don't throw an exception.
                if (valueToName[entityValue] != entityName)
                {
                    throw;
                }
            }
        }
        return valueToName;
    }
}
