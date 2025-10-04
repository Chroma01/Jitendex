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

internal partial class StrokeReader
{
    private readonly ILogger<StrokeReader> _logger;

    public StrokeReader(ILogger<StrokeReader> logger)
    {
        _logger = logger;
    }

    public void Read(XmlReader xmlReader, Element element)
    {
        var stroke = new Stroke
        {
            UnicodeScalarValue = element.UnicodeScalarValue,
            VariantTypeName = element.VariantTypeName,
            Id = GetStringAttribute(xmlReader, "id"),
            ElementId = element.Id,
            Order = element.Strokes.Count + 1,
            Type = xmlReader.GetAttribute("kvg:type"),
            PathData = GetStringAttribute(xmlReader, "d"),
            Element = element,
        };

        if (!xmlReader.IsEmptyElement)
        {
            LogNonEmptyElement(stroke.Id, element.Group.Entry.FileName());
        }

        element.Strokes.Add(stroke);
    }


    private string GetStringAttribute(XmlReader xmlReader, string name)
    {
        var attribute = xmlReader.GetAttribute(name);
        if (attribute is not null)
        {
            return attribute;
        }
        else
        {
            LogMissingAttribute(name, xmlReader.Name);
            return Guid.NewGuid().ToString();
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Unexpected element name `{Name}` in file `{FileName}`, parent ID `{ParentId}`")]
    private partial void LogUnexpectedElementName(string name, string fileName, string parentId);

    [LoggerMessage(LogLevel.Warning,
    "Stroke ID `{Id}` in file `{FileName}` is non-empty")]
    private partial void LogNonEmptyElement(string id, string fileName);

    [LoggerMessage(LogLevel.Warning,
    "Attribute `{AttributeName}` for element `{Name}` not found")]
    private partial void LogMissingAttribute(string attributeName, string name);
}
