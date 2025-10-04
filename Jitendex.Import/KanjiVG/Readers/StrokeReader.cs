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
            Id = GetAttribute(xmlReader, "id"),
            ElementId = element.Id,
            Order = element.Strokes.Count + 1,
            Type = xmlReader.GetAttribute("kvg:type"),
            PathData = GetAttribute(xmlReader, "d"),
            Element = element,
        };

        if (!xmlReader.IsEmptyElement)
        {
            // TODO: Log
        }

        element.Strokes.Add(stroke);
    }


    private static string GetAttribute(XmlReader xmlReader, string name)
    {
        var id = xmlReader.GetAttribute(name);
        if (id is not null)
        {
            return id;
        }
        else
        {
            // TODO: Log
            return Guid.NewGuid().ToString();
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Unexpected element name `{Name}` in file `{FileName}`, parent ID `{ParentId}`")]
    private partial void LogUnexpectedElementName(string name, string fileName, string parentId);
}
