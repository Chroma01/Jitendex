/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Kanjidic2.Import.Models;
using Jitendex.Kanjidic2.Import.Models.Groups;
using Jitendex.Kanjidic2.Import.Models.GroupElements;

namespace Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

internal partial class QueryCodeGroupReader : BaseReader<QueryCodeGroupReader>
{
    public QueryCodeGroupReader(ILogger<QueryCodeGroupReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    public async Task ReadAsync(Document document, EntryElement entry)
    {
        var group = new QueryCodeGroupElement
        {
            EntryId = entry.Id,
            Order = document.QueryCodeGroups.NextOrder(entry.Id),
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, entry, group);
                    break;
                case XmlNodeType.Text:
                    await LogUnexpectedTextNodeAsync(entry.Id, XmlTagName.QueryCodeGroup);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == XmlTagName.QueryCodeGroup;
                    break;
            }
        }

        document.QueryCodeGroups.Add(group.Key(), group);
    }

    private async Task ReadChildElementAsync(Document document, EntryElement entry, QueryCodeGroupElement group)
    {
        switch (_xmlReader.Name)
        {
            case XmlTagName.QueryCode:
                await ReadQueryCode(document, entry, group);
                break;
            default:
                LogUnexpectedChildElement(entry.ToRune(), _xmlReader.Name, XmlTagName.QueryCodeGroup);
                break;
        }
    }

    private async Task ReadQueryCode(Document document, EntryElement entry, QueryCodeGroupElement group)
    {
        var queryCode = new QueryCodeElement
        {
            EntryId = group.EntryId,
            GroupOrder = group.Order,
            Order = document.QueryCodes.NextOrder(group.Key()),
            TypeName = GetTypeName(document, entry),
            Misclassification = GetMisclassification(document),
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };
        document.QueryCodes.Add(queryCode.Key(), queryCode);
    }

    private string GetTypeName(Document document, EntryElement entry)
    {
        string typeName;
        var attribute = _xmlReader.GetAttribute("qc_type");
        if (string.IsNullOrWhiteSpace(attribute))
        {
            LogMissingTypeName(entry.ToRune());
            typeName = string.Empty;
        }
        else
        {
            typeName = attribute;
        }
        if (!document.QueryCodeTypes.ContainsKey(typeName))
        {
            var type = new QueryCodeTypeElement(typeName, document.Header.Date);
            document.QueryCodeTypes.Add(typeName, type);
        }
        return typeName;
    }

    private string? GetMisclassification(Document document)
    {
        var typeName = _xmlReader.GetAttribute("skip_misclass");
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }
        if (!document.MisclassificationTypes.ContainsKey(typeName))
        {
            var type = new MisclassificationTypeElement(typeName, document.Header.Date);
            document.MisclassificationTypes.Add(typeName, type);
        }
        return typeName;
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a query code type attribute")]
    partial void LogMissingTypeName(Rune character);
}
