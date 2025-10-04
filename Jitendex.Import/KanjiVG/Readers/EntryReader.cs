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
using System.Text.RegularExpressions;
using System.Globalization;

namespace Jitendex.Import.KanjiVG.Readers;

internal partial class EntryReader
{
    private readonly ILogger<EntryReader> _logger;
    private readonly ElementGroupReader _elementGroupReader;
    private readonly StrokeNumberGroupReader _strokeNumberGroupReader;

    public EntryReader(ILogger<EntryReader> logger, ElementGroupReader elementGroupReader, StrokeNumberGroupReader strokeNumberGroupReader)
    {
        _logger = logger;
        _elementGroupReader = elementGroupReader;
        _strokeNumberGroupReader = strokeNumberGroupReader;
    }

    public async Task<Entry?> ReadAsync(string fileName, XmlReader xmlReader)
    {
        var (unicodeScalarValue, variantTypeName) = Parse(fileName);
        if (unicodeScalarValue == default)
        {
            return null;
        }

        var entry = new Entry
        {
            UnicodeScalarValue = unicodeScalarValue,
            VariantTypeName = variantTypeName,
        };

        while (await xmlReader.ReadAsync())
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadElementAsync(xmlReader, entry);
                    break;
                case XmlNodeType.Text:
                    break;
                case XmlNodeType.DocumentType:
                    break;
            }
        }

        return entry;
    }

    private (int, string) Parse(string fileName)
    {
        Match match = FileNameRegex().Match(fileName);
        if (!match.Success)
        {
            _logger.LogError("Cannot parse filename {FileName}", fileName);
            return (default, string.Empty);
        }

        string variantTypeName = string.IsNullOrEmpty(match.Groups[2].Value)
            ? string.Empty
            : match.Groups[2].Value;

        if (int.TryParse(match.Groups[1].Value, NumberStyles.AllowHexSpecifier, provider: null, out int value))
        {
            return (value, variantTypeName);
        }
        else
        {
            _logger.LogError("Hex code in filename {FileName} is invalid", fileName);
            return (default, string.Empty);
        }
    }

    private async Task ReadElementAsync(XmlReader xmlReader, Entry entry)
    {
        switch (xmlReader.Name)
        {
            case "svg":
                break;
            case "g":
                await ReadGroupAsync(xmlReader, entry);
                break;
            default:
                LogUnexpectedElementName(xmlReader.Name, entry.FileName());
                break;
        }
    }

    private async Task ReadGroupAsync(XmlReader xmlReader, Entry entry)
    {
        var id = xmlReader.GetAttribute("id");
        if (id is null)
        {
            LogMissingGroupId(entry.FileName());
        }
        else if (id.StartsWith("kvg:StrokePaths"))
        {
            await _elementGroupReader.ReadAsync(xmlReader, entry);
        }
        else if (id.StartsWith("kvg:StrokeNumbers"))
        {
            await _strokeNumberGroupReader.ReadAsync(xmlReader, entry);
        }
        else
        {
            LogUnexpectedGroupIdPrefix(id, entry.FileName());
        }
    }

    [GeneratedRegex(pattern: @"^(.+?)(?:-(.+?))?\.svg$", RegexOptions.None)]
    private static partial Regex FileNameRegex();

    [LoggerMessage(LogLevel.Warning,
    "Unexpected element name `{Name}` in file `{FileName}`")]
    private partial void LogUnexpectedElementName(string name, string fileName);

    [LoggerMessage(LogLevel.Warning,
    "Group element in file `{FileName}` is missing an ID attribute")]
    private partial void LogMissingGroupId(string fileName);

    [LoggerMessage(LogLevel.Warning,
    "Unexpected group element ID `{Id}` in file `{FileName}`")]
    private partial void LogUnexpectedGroupIdPrefix(string id, string fileName);
}
