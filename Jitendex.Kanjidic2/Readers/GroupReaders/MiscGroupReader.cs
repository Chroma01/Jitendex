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

using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Kanjidic2.Models;
using Jitendex.Kanjidic2.Models.Groups;
using Jitendex.Kanjidic2.Models.EntryElements;

namespace Jitendex.Kanjidic2.Readers.GroupReaders;

internal partial class MiscGroupReader
{
    private const string GradeXmlTagName = "grade";
    private const string FrequencyXmlTagName = "freq";
    private const string JlptXmlTagName = "jlpt";

    private readonly ILogger<MiscGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;

    public MiscGroupReader(ILogger<MiscGroupReader> logger, XmlReader xmlReader, DocumentTypes docTypes) =>
        (_logger, _xmlReader, _docTypes) =
        (@logger, @xmlReader, @docTypes);

    public async Task<MiscGroup> ReadAsync(Entry entry)
    {
        var group = new MiscGroup
        {
            UnicodeScalarValue = entry.UnicodeScalarValue,
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
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), MiscGroup.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == MiscGroup.XmlTagName;
                    break;
            }
        }
        return group;
    }

    private async Task ReadChildElementAsync(MiscGroup group)
    {
        switch (_xmlReader.Name)
        {
            case GradeXmlTagName:
                await ReadGrade(group);
                break;
            case FrequencyXmlTagName:
                await ReadFrequency(group);
                break;
            case JlptXmlTagName:
                await ReadJlpt(group);
                break;
            case StrokeCount.XmlTagName:
                await ReadStrokeCount(group);
                break;
            case Variant.XmlTagName:
                await ReadVariant(group);
                break;
            case RadicalName.XmlTagName:
                await ReadRadicalName(group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, group.Entry.ToRune(), _xmlReader.Name, MiscGroup.XmlTagName);
                group.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadGrade(MiscGroup group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            group.Grade = value;
        }
        else
        {
            LogNonNumeric(group.Entry.ToRune(), GradeXmlTagName, text);
            group.Entry.IsCorrupt = true;
        }
    }

    private async Task ReadFrequency(MiscGroup group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            group.Frequency = value;
        }
        else
        {
            LogNonNumeric(group.Entry.ToRune(), FrequencyXmlTagName, text);
            group.Entry.IsCorrupt = true;
        }
    }

    private async Task ReadJlpt(MiscGroup group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            group.JlptLevel = value;
        }
        else
        {
            LogNonNumeric(group.Entry.ToRune(), JlptXmlTagName, text);
            group.Entry.IsCorrupt = true;
        }
    }

    private async Task ReadStrokeCount(MiscGroup group)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        int value;
        if (int.TryParse(text, out int x))
        {
            value = x;
        }
        else
        {
            LogNonNumeric(group.Entry.ToRune(), StrokeCount.XmlTagName, text);
            group.Entry.IsCorrupt = true;
            return;
        }
        var strokeCount = new StrokeCount
        {
            UnicodeScalarValue = group.UnicodeScalarValue,
            Order = group.StrokeCounts.Count + 1,
            Value = value,
            Entry = group.Entry,
        };
        group.StrokeCounts.Add(strokeCount);
    }

    private async Task ReadVariant(MiscGroup group)
    {
        var typeName = _xmlReader.GetAttribute("var_type");
        if (string.IsNullOrWhiteSpace(typeName))
        {
            LogMissingTypeName(group.Entry.ToRune());
            group.Entry.IsCorrupt = true;
        }
        var type = _docTypes.GetByName<VariantType>(typeName);
        var variant = new Variant
        {
            UnicodeScalarValue = group.UnicodeScalarValue,
            Order = group.Variants.Count + 1,
            TypeName = type.Name,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
            Entry = group.Entry,
            Type = type,
        };
        group.Variants.Add(variant);
    }

    private async Task ReadRadicalName(MiscGroup group)
    {
        var radicalName = new RadicalName
        {
            UnicodeScalarValue = group.UnicodeScalarValue,
            Order = group.RadicalNames.Count + 1,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
            Entry = group.Entry,
        };
        group.RadicalNames.Add(radicalName);
    }

#pragma warning disable IDE0060

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` contains a non-numeric <{TagName}> value : `{Text}")]
    partial void LogNonNumeric(Rune character, string tagName, string text);

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a variant type attribute")]
    partial void LogMissingTypeName(Rune character);

#pragma warning restore IDE0060

}
