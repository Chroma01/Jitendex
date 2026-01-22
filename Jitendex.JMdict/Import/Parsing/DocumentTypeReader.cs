/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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
using Jitendex.JMdict.Import.Models;

namespace Jitendex.JMdict.Import.Parsing;

internal partial class DocumentTypeReader : BaseReader<DocumentTypeReader>
{
    public DocumentTypeReader(ILogger<DocumentTypeReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    public async Task ReadAsync(Document document)
    {
        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    var dtd = await _xmlReader.GetValueAsync();
                    ParseEntities(document, dtd);
                    exit = true;
                    break;
                case XmlNodeType.Element:
                    LogUnexpectedElement(_xmlReader.Name);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    LogUnexpectedText(text);
                    break;
            }
        }
    }

    private void ParseEntities(Document document, string dtd)
    {
        foreach (Match match in DtdEntityRegex().Matches(dtd))
        {
            var name = match.Groups[1].Value;
            var description = match.Groups[2].Value;

            if (!document.KeywordDescriptionToName.TryGetValue(description, out var oldName))
            {
                document.KeywordDescriptionToName.Add(description, name);
            }
            else if (name != oldName)
            {
                _logger.LogError("Keyword `{Name}` has multiple descriptions in the Jmdict DTD", name);
            }
        }
    }

    [GeneratedRegex(@"<!ENTITY\s+(.*?)\s+""(.*?)"">", RegexOptions.None)]
    private static partial Regex DtdEntityRegex();

    [LoggerMessage(LogLevel.Warning,
    "Element <{xmlTagName}> encountered before the document type definitions have been parsed.")]
    partial void LogUnexpectedElement(string xmlTagName);

    [LoggerMessage(LogLevel.Warning,
    "Text node `{Text}` encountered before the document type definitions have been parsed.")]
    partial void LogUnexpectedText(string text);
}
