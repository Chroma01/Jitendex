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

using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Kanjidic2.Models;
using Jitendex.Kanjidic2.Models.Groups;
using Jitendex.Kanjidic2.Models.EntryElements;

namespace Jitendex.Kanjidic2.Readers.GroupReaders;

internal partial class QueryCodeGroupReader
{
    private readonly ILogger<QueryCodeGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;

    public QueryCodeGroupReader(ILogger<QueryCodeGroupReader> logger, XmlReader xmlReader, DocumentTypes docTypes) =>
        (_logger, _xmlReader, _docTypes) =
        (@logger, @xmlReader, @docTypes);

    public async Task<QueryCodeGroup> ReadAsync(Entry entry)
    {
        var group = new QueryCodeGroup
        {
            Character = entry.Character,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(group);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, entry.Character, QueryCodeGroup.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == QueryCodeGroup.XmlTagName;
                    break;
            }
        }
        return group;
    }

    private async Task ReadChildElementAsync(QueryCodeGroup group)
    {
        switch (_xmlReader.Name)
        {
            case QueryCode.XmlTagName:
                await ReadQueryCode(group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, group.Entry.Character, _xmlReader.Name, QueryCodeGroup.XmlTagName);
                group.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadQueryCode(QueryCodeGroup group)
    {
        var typeName = GetTypeName(group);
        var type = _docTypes.GetByName<QueryCodeType>(typeName);

        var misclassification = _xmlReader.GetAttribute("skip_misclass");
        var misclassificationType = misclassification is not null ?
            _docTypes.GetByName<MisclassificationType>(misclassification) :
            null;

        var queryCode = new QueryCode
        {
            Character = group.Character,
            Order = group.QueryCodes.Count + 1,
            TypeName = type.Name,
            Misclassification = misclassification,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
            Entry = group.Entry,
            Type = type,
            MisclassificationType = misclassificationType,
        };

        group.QueryCodes.Add(queryCode);
    }

    private string? GetTypeName(QueryCodeGroup group)
    {
        var typeName = _xmlReader.GetAttribute("qc_type");
        if (string.IsNullOrWhiteSpace(typeName))
        {
            LogMissingTypeName(group.Character);
            group.Entry.IsCorrupt = true;
        }
        return typeName;
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a query code type attribute")]
    private partial void LogMissingTypeName(string character);
}
