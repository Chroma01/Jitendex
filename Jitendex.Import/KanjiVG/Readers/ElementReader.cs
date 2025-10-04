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

internal partial class ElementReader
{
    private const string TextAttributeName = "kvg:element";
    private const string VariantAttributeName = "kvg:variant";
    private const string PartialAttributeName = "kvg:partial";
    private const string OriginalAttributeName = "kvg:original";
    private const string PartAttributeName = "kvg:part";
    private const string NumberAttributeName = "kvg:number";
    private const string TradFormAttributeName = "kvg:tradForm";
    private const string RadicalFormAttributeName = "kvg:radicalForm";
    private const string PositionAttributeName = "kvg:position";
    private const string RadicalAttributeName = "kvg:radical";
    private const string PhonAttributeName = "kvg:phon";

    private readonly ILogger<ElementReader> _logger;
    private readonly StrokeReader _strokeReader;

    public ElementReader(ILogger<ElementReader> logger, StrokeReader strokeReader)
    {
        _logger = logger;
        _strokeReader = strokeReader;
    }

    public async Task ReadAsync(XmlReader xmlReader, ElementGroup elementGroup)
    {
        var element = new Element
        {
            UnicodeScalarValue = elementGroup.Entry.UnicodeScalarValue,
            VariantTypeName = elementGroup.Entry.VariantTypeName,
            Id = GetStringAttribute(xmlReader, "id"),
            GroupId = elementGroup.Id,
            ParentId = null,
            Order = elementGroup.Elements.Count + 1,
            Text = xmlReader.GetAttribute(TextAttributeName),
            Variant = GetBooleanAttribute(xmlReader, VariantAttributeName),
            Partial = GetBooleanAttribute(xmlReader, PartialAttributeName),
            Original = xmlReader.GetAttribute(OriginalAttributeName),
            Part = GetNullableIntegerAttribute(xmlReader, PartAttributeName),
            Number = GetNullableIntegerAttribute(xmlReader, NumberAttributeName),
            TradForm = GetBooleanAttribute(xmlReader, TradFormAttributeName),
            RadicalForm = GetBooleanAttribute(xmlReader, RadicalFormAttributeName),
            Position = xmlReader.GetAttribute(PositionAttributeName),
            Radical = xmlReader.GetAttribute(RadicalAttributeName),
            Phon = xmlReader.GetAttribute(PhonAttributeName),
            Group = elementGroup,
            Parent = null,
        };

        bool exit = false;
        while (!exit && await xmlReader.ReadAsync())
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadXmlElementAsync(xmlReader, element);
                    break;
                case XmlNodeType.Text:
                    // TODO: Log
                    break;
                case XmlNodeType.EndElement:
                    exit = xmlReader.Name == "g";
                    break;
            }
        }

        elementGroup.Elements.Add(element);
    }

    private async Task ReadAsync(XmlReader xmlReader, Element parentElement)
    {
        var element = new Element
        {
            UnicodeScalarValue = parentElement.UnicodeScalarValue,
            VariantTypeName = parentElement.VariantTypeName,
            Id = GetStringAttribute(xmlReader, "id"),
            GroupId = parentElement.GroupId,
            ParentId = parentElement.Id,
            Order = parentElement.Children.Count + 1,
            Text = xmlReader.GetAttribute(TextAttributeName),
            Variant = GetBooleanAttribute(xmlReader, VariantAttributeName),
            Partial = GetBooleanAttribute(xmlReader, PartialAttributeName),
            Original = xmlReader.GetAttribute(OriginalAttributeName),
            Part = GetNullableIntegerAttribute(xmlReader, PartAttributeName),
            Number = GetNullableIntegerAttribute(xmlReader, NumberAttributeName),
            TradForm = GetBooleanAttribute(xmlReader, TradFormAttributeName),
            RadicalForm = GetBooleanAttribute(xmlReader, RadicalFormAttributeName),
            Position = xmlReader.GetAttribute(PositionAttributeName),
            Radical = xmlReader.GetAttribute(RadicalAttributeName),
            Phon = xmlReader.GetAttribute(PhonAttributeName),
            Group = parentElement.Group,
            Parent = parentElement,
        };

        bool exit = false;
        while (!exit && await xmlReader.ReadAsync())
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadXmlElementAsync(xmlReader, element);
                    break;
                case XmlNodeType.Text:
                    // TODO: Log
                    break;
                case XmlNodeType.EndElement:
                    exit = xmlReader.Name == "g";
                    break;
            }
        }

        parentElement.Children.Add(element);
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

    private int? GetNullableIntegerAttribute(XmlReader xmlReader, string name)
    {
        var attribute = xmlReader.GetAttribute(name);
        if (attribute is null)
        {
            return null;
        }
        else if (int.TryParse(attribute, out int value))
        {
            return value;
        }
        else
        {
            LogUnparsableText(name, xmlReader.Name);
            return null;
        }
    }

    private bool GetBooleanAttribute(XmlReader xmlReader, string name)
    {
        var attribute = xmlReader.GetAttribute(name);
        if (attribute is null)
        {
            return false;
        }
        else if (bool.TryParse(attribute, out bool value))
        {
            return value;
        }
        else
        {
            LogUnparsableText(name, xmlReader.Name);
            return false;
        }
    }

    private async Task ReadXmlElementAsync(XmlReader xmlReader, Element element)
    {
        switch (xmlReader.Name)
        {
            case "g":
                await ReadAsync(xmlReader, element);
                break;
            case Stroke.XmlTagName:
                _strokeReader.Read(xmlReader, element);
                break;
            default:
                LogUnexpectedElementName(xmlReader.Name, element.Group.Entry.FileName(), element.Id);
                break;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Unexpected element name `{Name}` in file `{FileName}`, parent ID `{ParentId}`")]
    private partial void LogUnexpectedElementName(string name, string fileName, string parentId);

    [LoggerMessage(LogLevel.Warning,
    "Attribute `{AttributeName}` for element `{Name}` not found")]
    private partial void LogMissingAttribute(string attributeName, string name);

    [LoggerMessage(LogLevel.Warning,
    "Attribute `{AttributeName}` for element `{Name}` cannot be parsed")]
    private partial void LogUnparsableText(string attributeName, string name);
}
