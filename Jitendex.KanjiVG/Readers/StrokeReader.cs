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
using Attributes = (string Id, string TypeText, string PathData);
using Jitendex.KanjiVG.Readers.Lookups;

namespace Jitendex.KanjiVG.Readers;

internal partial class StrokeReader
{
    private readonly ILogger<StrokeReader> _logger;
    private readonly StrokeTypeCache _strokeTypeCache;

    public StrokeReader(ILogger<StrokeReader> logger, StrokeTypeCache strokeTypeCache) =>
        (_logger, _strokeTypeCache) =
        (@logger, @strokeTypeCache);

    public void Read(XmlReader xmlReader, Component component)
    {
        var attributes = GetAttributes(xmlReader, component);

        var type = _strokeTypeCache.Get(attributes.TypeText);

        var stroke = new Stroke
        {
            UnicodeScalarValue = component.UnicodeScalarValue,
            VariantTypeId = component.VariantTypeId,
            GlobalOrder = component.Group.StrokeCount() + 1,
            LocalOrder = component.Strokes.Count + 1,
            ComponentGlobalOrder = component.GlobalOrder,
            TypeId = type.Id,
            PathData = attributes.PathData,
            Component = component,
            Type = type,
        };

        type.Strokes.Add(stroke);
        component.Strokes.Add(stroke);

        if (!xmlReader.IsEmptyElement)
        {
            LogNonEmptyElement(stroke.XmlIdAttribute(), component.Group.Entry.FileName());
        }

        if (!string.Equals(attributes.Id, stroke.XmlIdAttribute(), StringComparison.Ordinal))
        {
            LogWrongId(stroke.XmlIdAttribute(), attributes.Id, stroke.XmlIdAttribute());
        }
    }

    private Attributes GetAttributes(XmlReader xmlReader, Component component)
    {
        var attributes = new Attributes(null!, string.Empty, null!);

        int attributeCount = xmlReader.AttributeCount;
        for (int i = 0; i < attributeCount; i++)
        {
            xmlReader.MoveToAttribute(i);
            switch (xmlReader.Name)
            {
                case "id":
                    attributes.Id = xmlReader.Value;
                    break;
                case "kvg:type":
                    attributes.TypeText = xmlReader.Value;
                    break;
                case "d":
                    attributes.PathData = xmlReader.Value;
                    break;
                case "xmlns:kvg":
                    // Nothing to be done.
                    break;
                default:
                    LogUnknownAttributeName(xmlReader.Name, xmlReader.Value, component.Group.Entry.FileName());
                    break;
            }
        }

        if (attributeCount > 0)
        {
            xmlReader.MoveToElement();
        }

        if (attributes.Id is null)
        {
            LogMissingAttribute(nameof(attributes.Id), component.Group.Entry.FileName());
            attributes.Id = Guid.NewGuid().ToString();
        }

        if (attributes.PathData is null)
        {
            LogMissingAttribute(nameof(attributes.PathData), component.Group.Entry.FileName());
            attributes.Id = string.Empty;
        }

        return attributes;
    }

    [LoggerMessage(LogLevel.Warning,
    "Unknown component attribute name `{Name}` with value `{Value}` in file `{File}`")]
    private partial void LogUnknownAttributeName(string name, string value, string file);

    [LoggerMessage(LogLevel.Warning,
    "Stroke ID `{Id}` in file `{FileName}` is non-empty")]
    private partial void LogNonEmptyElement(string id, string fileName);

    [LoggerMessage(LogLevel.Warning,
    "Cannot find stroke attribute `{AttributeName}` in file `{File}`")]
    private partial void LogMissingAttribute(string attributeName, string file);

    [LoggerMessage(LogLevel.Warning,
    "{File}: Stroke ID `{Actual}` not equal to expected value `{Expected}`")]
    private partial void LogWrongId(string file, string actual, string expected);
}
