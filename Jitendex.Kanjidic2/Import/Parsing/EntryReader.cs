/*
Copyright (c) 2025-2026 Stephen Kraus
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
using Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

namespace Jitendex.Kanjidic2.Import.Parsing;

internal partial class EntryReader
{
    private readonly ILogger<EntryReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly CodepointGroupReader _codepointGroupReader;
    private readonly DictionaryGroupReader _dictionaryGroupReader;
    private readonly MiscGroupReader _miscGroupReader;
    private readonly QueryCodeGroupReader _queryCodeGroupReader;
    private readonly RadicalGroupReader _radicalGroupReader;
    private readonly ReadingMeaningGroupReader _readingMeaningGroupReader;

    public EntryReader(ILogger<EntryReader> logger, XmlReader xmlReader, CodepointGroupReader codepointGroupReader, DictionaryGroupReader dictionaryGroupReader, MiscGroupReader miscGroupReader, QueryCodeGroupReader queryCodeGroupReader, RadicalGroupReader radicalGroupReader, ReadingMeaningGroupReader readingMeaningGroupReader) =>
        (_logger, _xmlReader, _codepointGroupReader, _dictionaryGroupReader, _miscGroupReader, _queryCodeGroupReader, _radicalGroupReader, _readingMeaningGroupReader) =
        (@logger, @xmlReader, @codepointGroupReader, @dictionaryGroupReader, @miscGroupReader, @queryCodeGroupReader, @radicalGroupReader, @readingMeaningGroupReader);

    public async Task ReadAsync(Document document)
    {
        var entry = new Entry
        {
            UnicodeScalarValue = default,
            CreatedDate = document.FileHeader.Date
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, entry);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), Entry.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Entry.XmlTagName;
                    break;
            }
        }

        if (entry.UnicodeScalarValue != default)
        {
            document.Entries.Add(entry.UnicodeScalarValue, entry);
        }
        else
        {
            LogMissingCharacter(Entry.Character_XmlTagName);
        }
    }

    private async Task ReadChildElementAsync(Document document, Entry entry)
    {
        if (_xmlReader.Name != Entry.Character_XmlTagName && entry.UnicodeScalarValue == default)
        {
            LogPrematureElement(_xmlReader.Name);
            return;
        }

        switch (_xmlReader.Name)
        {
            case Entry.Character_XmlTagName:
                await ReadCharacterAsync(entry);
                break;
            case CodepointGroup.XmlTagName:
                await _codepointGroupReader.ReadAsync(document, entry);
                break;
            case DictionaryGroup.XmlTagName:
                await _dictionaryGroupReader.ReadAsync(document, entry);
                break;
            case MiscGroup.XmlTagName:
                await _miscGroupReader.ReadAsync(document, entry);
                break;
            case QueryCodeGroup.XmlTagName:
                await _queryCodeGroupReader.ReadAsync(document, entry);
                break;
            case RadicalGroup.XmlTagName:
                await _radicalGroupReader.ReadAsync(document, entry);
                break;
            case ReadingMeaningGroup.XmlTagName:
                await _readingMeaningGroupReader.ReadAsync(document, entry);
                break;
            default:
                Log.UnexpectedChildElement(_logger, entry.ToRune(), _xmlReader.Name, Entry.XmlTagName);
                break;
        }
    }

    private async Task ReadCharacterAsync(Entry entry)
    {
        if (entry.UnicodeScalarValue != default)
        {
            LogUnexpectedGroup(entry.ToRune(), Entry.Character_XmlTagName);
        }

        var text = await _xmlReader.ReadElementContentAsStringAsync();

        if (string.IsNullOrWhiteSpace(text))
        {
            LogEmptyElement(Entry.Character_XmlTagName);
            return;
        }

        var runes = text.EnumerateRunes().ToArray();

        if (runes.Length > 1)
        {
            LogMultipleCharacters(Entry.Character_XmlTagName, text);
        }

        entry.UnicodeScalarValue = runes[0].Value;
    }

    [LoggerMessage(LogLevel.Error,
    "Attempted to read <{XmlTagName}> child element before reading the entry primary key")]
    partial void LogPrematureElement(string xmlTagName);

    [LoggerMessage(LogLevel.Warning,
    "Entry contains no text in <{XmlTagName}> child element")]
    partial void LogEmptyElement(string xmlTagName);

    [LoggerMessage(LogLevel.Warning,
    "Entry contains more than one character in <{XmlTagName}> child element: `{Text}`")]
    partial void LogMultipleCharacters(string xmlTagName, string text);

    [LoggerMessage(LogLevel.Warning,
    "Entry for character `{Character}` has more than one <{XmlTagName}> child element")]
    partial void LogUnexpectedGroup(Rune character, string xmlTagName);

    [LoggerMessage(LogLevel.Error,
    "Entry contains no <{XmlTagName}> element; no primary key can be assigned")]
    partial void LogMissingCharacter(string xmlTagName);
}
