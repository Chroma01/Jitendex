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
using Jitendex.Import.KanjiVG.Models;

namespace Jitendex.Import.KanjiVG.Readers;

internal partial class StrokeNumberGroupReader
{
    private readonly ILogger<StrokeNumberGroupReader> _logger;
    private readonly StrokeNumberReader _strokeNumberReader;

    public StrokeNumberGroupReader(ILogger<StrokeNumberGroupReader> logger, StrokeNumberReader strokeNumberReader)
    {
        _logger = logger;
        _strokeNumberReader = strokeNumberReader;
    }

    public async Task ReadAsync(XmlReader xmlReader, Entry entry)
    {
        var strokeNumberGroup = new StrokeNumberGroup
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            VariantTypeName = entry.VariantTypeName,
            Id = xmlReader.GetAttribute("id")!,
            Style = xmlReader.GetAttribute("style"),
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
                    // TODO: Log
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
    "Unexpected element name `{Name}` in file `{FileName}`, parent ID `{ParentId}`")]
    private partial void LogUnexpectedElementName(string name, string fileName, string parentId);

    [LoggerMessage(LogLevel.Warning,
    "File `{FileName}` contains multiple stroke number groups")]
    private partial void LogMultipleGroups(string fileName);
}
