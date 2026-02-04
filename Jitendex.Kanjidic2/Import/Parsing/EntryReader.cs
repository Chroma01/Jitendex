/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Kanjidic2.Import.Models;
using Jitendex.Kanjidic2.Import.Models.Groups;
using Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

namespace Jitendex.Kanjidic2.Import.Parsing;

internal partial class EntryReader : BaseReader<EntryReader>
{
    private readonly CodepointGroupReader _codepointGroupReader;
    private readonly DictionaryGroupReader _dictionaryGroupReader;
    private readonly MiscGroupReader _miscGroupReader;
    private readonly QueryCodeGroupReader _queryCodeGroupReader;
    private readonly RadicalGroupReader _radicalGroupReader;
    private readonly ReadingMeaningGroupReader _readingMeaningGroupReader;

    public EntryReader(ILogger<EntryReader> logger, XmlReader xmlReader, CodepointGroupReader codepointGroupReader, DictionaryGroupReader dictionaryGroupReader, MiscGroupReader miscGroupReader, QueryCodeGroupReader queryCodeGroupReader, RadicalGroupReader radicalGroupReader, ReadingMeaningGroupReader readingMeaningGroupReader) : base(logger, xmlReader) =>
        (_codepointGroupReader, _dictionaryGroupReader, _miscGroupReader, _queryCodeGroupReader, _radicalGroupReader, _readingMeaningGroupReader) =
        (@codepointGroupReader, @dictionaryGroupReader, @miscGroupReader, @queryCodeGroupReader, @radicalGroupReader, @readingMeaningGroupReader);

    public async Task ReadAsync(Document document)
    {
        var entry = new EntryElement
        {
            Id = default
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
                    await LogUnexpectedTextNodeAsync(entry.Id, EntryElement.XmlTagName);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == EntryElement.XmlTagName;
                    break;
            }
        }

        if (entry.Id != default)
        {
            document.Entries.Add(entry.Id, entry);
        }
        else
        {
            LogMissingCharacter(EntryElement.Character_XmlTagName);
        }
    }

    private async Task ReadChildElementAsync(Document document, EntryElement entry)
    {
        if (_xmlReader.Name != EntryElement.Character_XmlTagName && entry.Id == default)
        {
            LogPrematureElement(_xmlReader.Name);
            return;
        }

        switch (_xmlReader.Name)
        {
            case EntryElement.Character_XmlTagName:
                await ReadCharacterAsync(entry);
                break;
            case CodepointGroupElement.XmlTagName:
                await _codepointGroupReader.ReadAsync(document, entry);
                break;
            case DictionaryGroupElement.XmlTagName:
                await _dictionaryGroupReader.ReadAsync(document, entry);
                break;
            case MiscGroupElement.XmlTagName:
                await _miscGroupReader.ReadAsync(document, entry);
                break;
            case QueryCodeGroupElement.XmlTagName:
                await _queryCodeGroupReader.ReadAsync(document, entry);
                break;
            case RadicalGroupElement.XmlTagName:
                await _radicalGroupReader.ReadAsync(document, entry);
                break;
            case ReadingMeaningGroupElement.XmlTagName:
                await _readingMeaningGroupReader.ReadAsync(document, entry);
                break;
            default:
                LogUnexpectedChildElement(entry.ToRune(), _xmlReader.Name, EntryElement.XmlTagName);
                break;
        }
    }

    private async Task ReadCharacterAsync(EntryElement entry)
    {
        if (entry.Id != default)
        {
            LogUnexpectedGroup(entry.ToRune(), EntryElement.Character_XmlTagName);
        }

        var text = await _xmlReader.ReadElementContentAsStringAsync();

        if (string.IsNullOrWhiteSpace(text))
        {
            LogEmptyElement(EntryElement.Character_XmlTagName);
            return;
        }

        var runes = text.EnumerateRunes().ToArray();

        if (runes.Length > 1)
        {
            LogMultipleCharacters(EntryElement.Character_XmlTagName, text);
        }

        entry.Id = runes[0].Value;
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
