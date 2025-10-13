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

using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.KanjiVG.Models;
using Jitendex.KanjiVG.Readers.Lookups;

namespace Jitendex.KanjiVG.Readers;

internal partial class EntryReader
{
    private readonly ILogger<EntryReader> _logger;
    private readonly ComponentGroupReader _componentGroupReader;
    private readonly StrokeNumberGroupReader _strokeNumberGroupReader;
    private readonly VariantTypeCache _variantTypeCache;

    public EntryReader(
        ILogger<EntryReader> logger,
        ComponentGroupReader componentGroupReader,
        StrokeNumberGroupReader strokeNumberGroupReader,
        VariantTypeCache variantTypeCache) =>
        (_logger, _componentGroupReader, _strokeNumberGroupReader, _variantTypeCache) =
        (@logger, @componentGroupReader, @strokeNumberGroupReader, @variantTypeCache);

    public async Task<Entry?> ReadAsync(string fileName, XmlReader xmlReader)
    {
        var (unicodeScalarValue, variantTypeName) = Parse(fileName);
        if (unicodeScalarValue == default)
        {
            return null;
        }

        var variantType = _variantTypeCache.Get(variantTypeName);

        var entry = new Entry
        {
            UnicodeScalarValue = unicodeScalarValue,
            VariantTypeId = variantType.Id,
            ComponentGroup = null!,
            StrokeNumberGroup = null!,
            VariantType = variantType,
        };

        while (await xmlReader.ReadAsync())
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(xmlReader, entry);
                    break;
                case XmlNodeType.Text:
                    var text = await xmlReader.GetValueAsync();
                    LogUnexpectedTextNode(fileName, text);
                    break;
                case XmlNodeType.DocumentType:
                    break;
            }
        }

        if (entry.ComponentGroup is null || entry.StrokeNumberGroup is null)
        {
            if (entry.ComponentGroup is null)
            {
                LogMissingGroup(nameof(entry.ComponentGroup), fileName);
            }
            if (entry.StrokeNumberGroup is null)
            {
                LogMissingGroup(nameof(entry.StrokeNumberGroup), fileName);
            }
            return null;
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
        else if (int.TryParse(match.Groups[1].Value, NumberStyles.AllowHexSpecifier, provider: null, out int value))
        {
            return (value, match.Groups[2].Value);
        }
        else
        {
            _logger.LogError("Hex code in filename {FileName} is invalid", fileName);
            return (default, string.Empty);
        }
    }

    private async Task ReadChildElementAsync(XmlReader xmlReader, Entry entry)
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
            await _componentGroupReader.ReadAsync(xmlReader, entry);
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
    "{File}: Unexpected XML text node `{Text}`")]
    public partial void LogUnexpectedTextNode(string file, string text);

    [LoggerMessage(LogLevel.Warning,
    "Unexpected element name `{Name}` in file `{FileName}`")]
    private partial void LogUnexpectedElementName(string name, string fileName);

    [LoggerMessage(LogLevel.Warning,
    "No `{GroupName}` group found in file `{FileName}`")]
    private partial void LogMissingGroup(string groupName, string fileName);

    [LoggerMessage(LogLevel.Warning,
    "Group element in file `{FileName}` is missing an ID attribute")]
    private partial void LogMissingGroupId(string fileName);

    [LoggerMessage(LogLevel.Warning,
    "Unexpected group element ID `{Id}` in file `{FileName}`")]
    private partial void LogUnexpectedGroupIdPrefix(string id, string fileName);
}
