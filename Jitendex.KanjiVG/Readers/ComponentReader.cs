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

internal partial class ComponentReader
{
    private readonly ILogger<ComponentReader> _logger;
    private readonly ComponentAttributesReader _attributesReader;
    private readonly StrokeReader _strokeReader;

    public ComponentReader(ILogger<ComponentReader> logger, ComponentAttributesReader attributesReader, StrokeReader strokeReader) =>
        (_logger, _attributesReader, _strokeReader) =
        (@logger, @attributesReader, @strokeReader);

    public async Task ReadAsync(XmlReader xmlReader, ComponentGroup group)
    {
        var attributes = _attributesReader.Read(xmlReader, group);

        var component = new Component
        {
            UnicodeScalarValue = group.Entry.UnicodeScalarValue,
            VariantTypeName = group.Entry.VariantTypeName,
            Id = attributes.Id,
            ParentId = null,
            Order = group.Components.Count + 1,
            Text = attributes.Text,
            Variant = attributes.Variant,
            Partial = attributes.Partial,
            Original = attributes.Original,
            Part = attributes.Part,
            Number = attributes.Number,
            TradForm = attributes.TradForm,
            RadicalForm = attributes.RadicalForm,
            Position = attributes.Position,
            Radical = attributes.Radical,
            Phon = attributes.Phon,
            Group = group,
            Parent = null,
        };

        bool exit = false;
        while (!exit && await xmlReader.ReadAsync())
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(xmlReader, component);
                    break;
                case XmlNodeType.Text:
                    var text = await xmlReader.GetValueAsync();
                    LogUnexpectedTextNode(group.Entry.FileName(), text);
                    break;
                case XmlNodeType.EndElement:
                    exit = xmlReader.Name == "g";
                    break;
            }
        }

        group.Components.Add(component);
    }

    private async Task ReadAsync(XmlReader xmlReader, Component parent)
    {
        var attributes = _attributesReader.Read(xmlReader, parent.Group);

        var component = new Component
        {
            UnicodeScalarValue = parent.UnicodeScalarValue,
            VariantTypeName = parent.VariantTypeName,
            Id = attributes.Id,
            ParentId = parent.Id,
            Order = parent.Children.Count + 1,
            Text = attributes.Text,
            Variant = attributes.Variant,
            Partial = attributes.Partial,
            Original = attributes.Original,
            Part = attributes.Part,
            Number = attributes.Number,
            TradForm = attributes.TradForm,
            RadicalForm = attributes.RadicalForm,
            Position = attributes.Position,
            Radical = attributes.Radical,
            Phon = attributes.Phon,
            Group = parent.Group,
            Parent = parent,
        };

        bool exit = false;
        while (!exit && await xmlReader.ReadAsync())
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(xmlReader, component);
                    break;
                case XmlNodeType.Text:
                    var text = await xmlReader.GetValueAsync();
                    LogUnexpectedTextNode(parent.Group.Entry.FileName(), text);
                    break;
                case XmlNodeType.EndElement:
                    exit = xmlReader.Name == "g";
                    break;
            }
        }

        parent.Children.Add(component);
    }

    private async Task ReadChildElementAsync(XmlReader xmlReader, Component component)
    {
        switch (xmlReader.Name)
        {
            case "g":
                await ReadAsync(xmlReader, component);
                break;
            case "path":
                _strokeReader.Read(xmlReader, component);
                break;
            default:
                LogUnexpectedComponentName(xmlReader.Name, component.Group.Entry.FileName(), component.Id);
                break;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "{File}: Unexpected XML text node `{Text}`")]
    public partial void LogUnexpectedTextNode(string file, string text);

    [LoggerMessage(LogLevel.Warning,
    "Unexpected component child name `{Name}` in file `{FileName}`, parent ID `{ParentId}`")]
    private partial void LogUnexpectedComponentName(string name, string fileName, string parentId);
}
