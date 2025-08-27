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

internal partial class DocumentTypesReader : IJmdictReader<NoParent, NoChild>
{
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;
    private readonly ILogger<DocumentTypesReader> _logger;

    public DocumentTypesReader(XmlReader xmlReader, DocumentTypes docTypes, ILogger<DocumentTypesReader> logger) =>
        (_xmlReader, _docTypes, _logger) =
        (@xmlReader, @docTypes, @logger);

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
                    RegisterKeywords(dtd);
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

    private void RegisterKeywords(string dtd)
    {
        // Entities explicitly defined in document header.
        foreach (var (name, description) in ParseEntities(dtd))
        {
            // Since there's no keyword overlap between these types,
            // it's fine to register all the definitions for all of the types.
            _docTypes.RegisterKeyword<ReadingInfoTag>(name, description);
            _docTypes.RegisterKeyword<KanjiFormInfoTag>(name, description);
            _docTypes.RegisterKeyword<PartOfSpeechTag>(name, description);
            _docTypes.RegisterKeyword<FieldTag>(name, description);
            _docTypes.RegisterKeyword<MiscTag>(name, description);
            _docTypes.RegisterKeyword<DialectTag>(name, description);
        }

        // Entities implicitly defined that cannot be parsed from the document.
        foreach (var (name, description) in GlossType.NameToDescription)
            _docTypes.RegisterKeyword<GlossType>(name, description);

        foreach (var (name, description) in CrossReferenceType.NameToDescription)
            _docTypes.RegisterKeyword<CrossReferenceType>(name, description);

        foreach (var (name, description) in LanguageSourceType.NameToDescription)
            _docTypes.RegisterKeyword<LanguageSourceType>(name, description);

        foreach (var (name, description) in ExampleSourceType.NameToDescription)
            _docTypes.RegisterKeyword<ExampleSourceType>(name, description);

        foreach (var (name, description) in PriorityTag.NameToDescription)
            _docTypes.RegisterKeyword<PriorityTag>(name, description);

        foreach (var (name, description) in Language.NameToDescription)
            _docTypes.RegisterKeyword<Language>(name, description);
    }
}
