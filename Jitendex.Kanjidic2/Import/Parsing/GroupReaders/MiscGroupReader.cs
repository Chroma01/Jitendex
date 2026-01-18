/*
Copyright (c) 2025 Stephen Kraus
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

using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Kanjidic2.Import.Models;
using Jitendex.Kanjidic2.Import.Models.Groups;
using Jitendex.Kanjidic2.Import.Models.GroupElements;

namespace Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

internal partial class MiscGroupReader
{
    private readonly ILogger<MiscGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly Dictionary<int, int> _usedGroupOrders = [];
    private readonly Dictionary<(int, int), int> _usedStrokeCountOrders = [];
    private readonly Dictionary<(int, int), int> _usedVariantOrders = [];
    private readonly Dictionary<(int, int), int> _usedRadicalNameOrders = [];

    public MiscGroupReader(ILogger<MiscGroupReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task ReadAsync(Document document, Entry entry)
    {
        var group = new MiscGroup
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            Order = _usedGroupOrders.TryGetValue(entry.UnicodeScalarValue, out var order) ? order + 1 : 0,
        };

        _usedGroupOrders[entry.UnicodeScalarValue] = group.Order;

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, entry, group);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), MiscGroup.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == MiscGroup.XmlTagName;
                    break;
            }
        }

        document.MiscGroups.Add(group.Key(), group);
    }

    private async Task ReadChildElementAsync(Document document, Entry entry, MiscGroup group)
    {
        switch (_xmlReader.Name)
        {
            case MiscGroup.Grade_XmlTagName:
                await ReadGrade(entry, group);
                break;
            case MiscGroup.Frequency_XmlTagName:
                await ReadFrequency(entry, group);
                break;
            case MiscGroup.Jlpt_XmlTagName:
                await ReadJlpt(entry, group);
                break;
            case StrokeCount.XmlTagName:
                await ReadStrokeCount(document, entry, group);
                break;
            case Variant.XmlTagName:
                await ReadVariant(document, entry, group);
                break;
            case RadicalName.XmlTagName:
                await ReadRadicalName(document, entry, group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, entry.ToRune(), _xmlReader.Name, MiscGroup.XmlTagName);
                break;
        }
    }

    private async Task ReadGrade(Entry entry, MiscGroup group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            group.Grade = value;
        }
        else
        {
            LogNonNumeric(entry.ToRune(), MiscGroup.Grade_XmlTagName, text);
        }
    }

    private async Task ReadFrequency(Entry entry, MiscGroup group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            group.Frequency = value;
        }
        else
        {
            LogNonNumeric(entry.ToRune(), MiscGroup.Frequency_XmlTagName, text);
        }
    }

    private async Task ReadJlpt(Entry entry, MiscGroup group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            group.JlptLevel = value;
        }
        else
        {
            LogNonNumeric(entry.ToRune(), MiscGroup.Jlpt_XmlTagName, text);
        }
    }

    private async Task ReadStrokeCount(Document document, Entry entry, MiscGroup group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        int value;
        if (int.TryParse(text, out int x))
        {
            value = x;
        }
        else
        {
            LogNonNumeric(entry.ToRune(), StrokeCount.XmlTagName, text);
            return;
        }
        var strokeCount = new StrokeCount
        {
            UnicodeScalarValue = group.UnicodeScalarValue,
            GroupOrder = group.Order,
            Order = _usedStrokeCountOrders.TryGetValue(group.Key(), out var order) ? order + 1 : 0,
            Value = value,
        };
        _usedStrokeCountOrders[group.Key()] = strokeCount.Order;
        document.StrokeCounts.Add(strokeCount.Key(), strokeCount);
    }

    private async Task ReadVariant(Document document, Entry entry, MiscGroup group)
    {
        var variant = new Variant
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            GroupOrder = group.Order,
            Order = _usedVariantOrders.TryGetValue(group.Key(), out var order) ? order + 1 : 0,
            TypeName = GetVariantTypeName(document, entry),
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };
        _usedVariantOrders[group.Key()] = variant.Order;
        document.Variants.Add(variant.Key(), variant);
    }

    private string GetVariantTypeName(Document document, Entry entry)
    {
        string typeName;
        var attribute = _xmlReader.GetAttribute(Variant.TypeName_XmlAttrName);
        if (string.IsNullOrWhiteSpace(attribute))
        {
            LogMissingTypeName(entry.ToRune());
            typeName = string.Empty;
        }
        else
        {
            typeName = attribute;
        }
        if (!document.VariantTypes.ContainsKey(typeName))
        {
            var type = new VariantType
            {
                Name = typeName,
                CreatedDate = document.FileHeader.DateOfCreation,
            };
            document.VariantTypes.Add(typeName, type);
        }
        return typeName;
    }

    private async Task ReadRadicalName(Document document, Entry entry, MiscGroup group)
    {
        var radicalName = new RadicalName
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
            GroupOrder = group.Order,
            Order = _usedRadicalNameOrders.TryGetValue(group.Key(), out var order) ? order + 1 : 0,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };
        _usedRadicalNameOrders[group.Key()] = radicalName.Order;
        document.RadicalNames.Add(radicalName.Key(), radicalName);
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` contains a non-numeric <{TagName}> value : `{Text}")]
    partial void LogNonNumeric(Rune character, string tagName, string text);

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a variant type attribute")]
    partial void LogMissingTypeName(Rune character);
}
