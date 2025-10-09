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
using Jitendex.Import.Kanjidic2.Models;
using Jitendex.Import.Kanjidic2.Models.Groups;
using Jitendex.Import.Kanjidic2.Models.EntryElements;

namespace Jitendex.Import.Kanjidic2.Readers.GroupReaders;

internal partial class DictionaryGroupReader
{
    private readonly ILogger<DictionaryGroupReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;

    public DictionaryGroupReader(ILogger<DictionaryGroupReader> logger, XmlReader xmlReader, DocumentTypes docTypes) =>
        (_logger, _xmlReader, _docTypes) =
        (@logger, @xmlReader, @docTypes);

    public async Task<DictionaryGroup> ReadAsync(Entry entry)
    {
        var group = new DictionaryGroup
        {
            Character = entry.Character,
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
                    Log.UnexpectedTextNode(_logger, entry.Character, DictionaryGroup.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == DictionaryGroup.XmlTagName;
                    break;
            }
        }
        return group;
    }

    private async Task ReadChildElementAsync(DictionaryGroup group)
    {
        switch (_xmlReader.Name)
        {
            case Dictionary.XmlTagName:
                await ReadDictionary(group);
                break;
            default:
                Log.UnexpectedChildElement(_logger, group.Entry.Character, _xmlReader.Name, DictionaryGroup.XmlTagName);
                group.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadDictionary(DictionaryGroup group)
    {
        var typeName = GetTypeName(group);
        var type = _docTypes.GetByName<DictionaryType>(typeName);

        var dictionary = new Dictionary
        {
            Character = group.Character,
            Order = group.Dictionaries.Count + 1,
            TypeName = type.Name,
            Volume = GetDictionaryVolume(group),
            Page = GetDictionaryPage(group),
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
            Entry = group.Entry,
            Type = type,
        };
        group.Dictionaries.Add(dictionary);
    }

    private string? GetTypeName(DictionaryGroup group)
    {
        var typeName = _xmlReader.GetAttribute("dr_type");
        if (string.IsNullOrWhiteSpace(typeName))
        {
            LogMissingTypeName(group.Character);
            group.Entry.IsCorrupt = true;
        }
        return typeName;
    }

    private int? GetDictionaryVolume(DictionaryGroup group)
    {
        var volume = _xmlReader.GetAttribute("m_vol");
        if (volume is null)
        {
            // Not an error; allowed to be null
            return null;
        }
        if (int.TryParse(volume, out int value))
        {
            return value;
        }
        else
        {
            LogNonNumericVolume(group.Character, volume);
            group.Entry.IsCorrupt = true;
            return null;
        }
    }

    private int? GetDictionaryPage(DictionaryGroup group)
    {
        var page = _xmlReader.GetAttribute("m_page");
        if (page is null)
        {
            // Not an error; allowed to be null
            return null;
        }
        if (int.TryParse(page, out int value))
        {
            return value;
        }
        else
        {
            LogNonNumericPage(group.Character, page);
            group.Entry.IsCorrupt = true;
            return null;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a dictionary type attribute")]
    private partial void LogMissingTypeName(string character);

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` has a dictionary volume attribute that is non-numeric: `{Volume}`")]
    private partial void LogNonNumericVolume(string character, string volume);

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` has a dictionary page attribute that is non-numeric: `{Page}`")]
    private partial void LogNonNumericPage(string character, string page);
}
