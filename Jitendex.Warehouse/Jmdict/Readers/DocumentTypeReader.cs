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
using Microsoft.Extensions.Logging;
using Jitendex.Warehouse.Jmdict.Models;

namespace Jitendex.Warehouse.Jmdict.Readers;

internal partial class DocumentTypeReader : IJmdictReader<NoParent, NoChild>
{
    private readonly XmlReader _xmlReader;
    private readonly EntityFactory _factory;
    private readonly ILogger<DocumentTypeReader> _logger;

    public DocumentTypeReader(XmlReader xmlReader, EntityFactory factory, ILogger<DocumentTypeReader> logger) =>
        (_xmlReader, _factory, _logger) =
        (xmlReader, factory, logger);

    public async Task<NoChild> ReadAsync(NoParent noParent)
    {
        var @void = new NoChild();
        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    var dtd = await _xmlReader.GetValueAsync();
                    RegisterFactoryKeywords(dtd);
                    exit = true;
                    break;
                case XmlNodeType.Element:
                    _logger.LogError("Unexpected element node found in document preamble.");
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    _logger.LogError($"Unexpected text node found in document preamble: `{text}`");
                    break;
            }
        }
        return @void;
    }

    [GeneratedRegex(@"<!ENTITY\s+(.*?)\s+""(.*?)"">", RegexOptions.None)]
    private static partial Regex DtdEntityRegex();

    private Dictionary<string, string> ParseEntities(string dtd)
    {
        var entityNameToDescription = new Dictionary<string, string>();
        foreach (Match match in DtdEntityRegex().Matches(dtd))
        {
            var name = match.Groups[1].Value;
            var description = match.Groups[2].Value;
            try
            {
                entityNameToDescription.Add(name, description);
            }
            catch (ArgumentException)
            {
                if (entityNameToDescription[name] == description)
                {
                    // Ignore repeated definitions that are exact duplicates.
                }
                else
                {
                    throw;
                }
            }
        }
        return entityNameToDescription;
    }

    private void RegisterFactoryKeywords(string dtd)
    {
        // Entities explicitly defined in document header.
        var nameToDescription = ParseEntities(dtd);
        foreach (var (name, description) in nameToDescription)
        {
            // Since there's no keyword overlap between these types,
            // it's fine to register all the definitions for all of the types.
            _factory.RegisterKeyword<ReadingInfoTag>(name, description);
            _factory.RegisterKeyword<KanjiFormInfoTag>(name, description);
            _factory.RegisterKeyword<PartOfSpeechTag>(name, description);
            _factory.RegisterKeyword<FieldTag>(name, description);
            _factory.RegisterKeyword<MiscTag>(name, description);
            _factory.RegisterKeyword<DialectTag>(name, description);
        }

        // Entities implicitly defined that cannot be parsed from the document.
        foreach (var (name, description) in GlossType.NameToDescription)
            _factory.RegisterKeyword<GlossType>(name, description);

        foreach (var (name, description) in CrossReferenceType.NameToDescription)
            _factory.RegisterKeyword<CrossReferenceType>(name, description);

        foreach (var (name, description) in LanguageSourceType.NameToDescription)
            _factory.RegisterKeyword<LanguageSourceType>(name, description);

        foreach (var (name, description) in ExampleSourceType.NameToDescription)
            _factory.RegisterKeyword<ExampleSourceType>(name, description);

        foreach (var (name, description) in PriorityTag.NameToDescription)
            _factory.RegisterKeyword<PriorityTag>(name, description);
    }
}
