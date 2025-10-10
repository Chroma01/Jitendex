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
using Jitendex.KanjiVG.Models;

namespace Jitendex.KanjiVG.Readers;

internal partial class StrokeNumberGroupReader
{
    private readonly ILogger<StrokeNumberGroupReader> _logger;
    private readonly StrokeNumberReader _strokeNumberReader;

    public StrokeNumberGroupReader(ILogger<StrokeNumberGroupReader> logger, StrokeNumberReader strokeNumberReader) =>
        (_logger, _strokeNumberReader) =
        (@logger, @strokeNumberReader);

    public async Task ReadAsync(XmlReader xmlReader, Entry entry)
    {
        var (id, style) = GetAttributes(xmlReader, entry);

        var strokeNumberGroup = new StrokeNumberGroup
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            VariantTypeName = entry.VariantTypeName,
            Id = id,
            Style = style,
            Entry = entry,
        };

        bool exit = false;
        while (!exit && await xmlReader.ReadAsync())
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadElementAsync(xmlReader, strokeNumberGroup);
                    break;
                case XmlNodeType.Text:
                    var text = await xmlReader.GetValueAsync();
                    LogUnexpectedTextNode(entry.FileName(), text);
                    break;
                case XmlNodeType.EndElement:
                    exit = xmlReader.Name == "g";
                    break;
            }
        }

        if (entry.StrokeNumberGroup is null)
        {
            entry.StrokeNumberGroup = strokeNumberGroup;
        }
        else
        {
            LogMultipleGroups(entry.FileName());
        }
    }

    private (string Id, string? Style) GetAttributes(XmlReader xmlReader, Entry entry)
    {
        string id = null!;
        string? style = null;

        int attributeCount = xmlReader.AttributeCount;
        for (int i = 0; i < attributeCount; i++)
        {
            xmlReader.MoveToAttribute(i);
            switch (xmlReader.Name)
            {
                case "id":
                    id = xmlReader.Value;
                    break;
                case "style":
                    style = xmlReader.Value;
                    break;
                case "xmlns:kvg":
                    // Nothing to be done.
                    break;
                default:
                    LogUnknownAttributeName(xmlReader.Name, xmlReader.Value, entry.FileName());
                    break;
            }
        }

        if (attributeCount > 0)
        {
            xmlReader.MoveToElement();
        }

        if (id is null)
        {
            LogMissingId(entry.FileName());
            id = Guid.NewGuid().ToString();
        }

        return (id, style);
    }

    private async Task ReadElementAsync(XmlReader xmlReader, StrokeNumberGroup strokeNumberGroup)
    {
        switch (xmlReader.Name)
        {
            case "text":
                await _strokeNumberReader.ReadAsync(xmlReader, strokeNumberGroup);
                break;
            default:
                LogUnexpectedElementName(xmlReader.Name, strokeNumberGroup.Entry.FileName(), strokeNumberGroup.Id);
                break;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Unknown component attribute name `{Name}` with value `{Value}` in file `{File}`")]
    private partial void LogUnknownAttributeName(string name, string value, string file);

    [LoggerMessage(LogLevel.Warning,
    "Cannot find component group ID attribute in file `{File}`")]
    private partial void LogMissingId(string file);

    [LoggerMessage(LogLevel.Warning,
    "{File}: Unexpected XML text node `{Text}`")]
    public partial void LogUnexpectedTextNode(string file, string text);

    [LoggerMessage(LogLevel.Warning,
    "Unexpected element name `{Name}` in file `{FileName}`, parent ID `{ParentId}`")]
    private partial void LogUnexpectedElementName(string name, string fileName, string parentId);

    [LoggerMessage(LogLevel.Warning,
    "File `{FileName}` contains multiple stroke number groups")]
    private partial void LogMultipleGroups(string fileName);
}
