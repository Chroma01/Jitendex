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
using Jitendex.Warehouse.Kanjidic2.Models;
using Jitendex.Warehouse.Kanjidic2.Models.Groups;
using Jitendex.Warehouse.Kanjidic2.Readers.GroupReaders;

namespace Jitendex.Warehouse.Kanjidic2.Readers;

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

    public async Task<Entry> ReadAsync()
    {
        var entry = new Entry
        {
            Character = string.Empty,
            IsKokuji = false,
            IsGhost = false,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(entry);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, entry.Character, Entry.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Entry.XmlTagName;
                    break;
            }
        }
        return entry;
    }

    private async Task ReadChildElementAsync(Entry entry)
    {
        switch (_xmlReader.Name)
        {
            case Entry.Character_XmlTagName:
                await ReadCharacterAsync(entry);
                break;
            case CodepointGroup.XmlTagName:
                if (entry.CodepointGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.Character, CodepointGroup.XmlTagName);
                }
                entry.CodepointGroup = await _codepointGroupReader.ReadAsync(entry);
                entry.Codepoints = entry.CodepointGroup.Codepoints;
                break;
            case DictionaryGroup.XmlTagName:
                if (entry.DictionaryGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.Character, DictionaryGroup.XmlTagName);
                }
                entry.DictionaryGroup = await _dictionaryGroupReader.ReadAsync(entry);
                entry.Dictionaries = entry.DictionaryGroup.Dictionaries;
                break;
            case MiscGroup.XmlTagName:
                if (entry.MiscGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.Character, MiscGroup.XmlTagName);
                }
                entry.MiscGroup = await _miscGroupReader.ReadAsync(entry);
                entry.Grade = entry.MiscGroup.Grade;
                entry.Frequency = entry.MiscGroup.Frequency;
                entry.JlptLevel = entry.MiscGroup.JlptLevel;
                entry.StrokeCounts = entry.MiscGroup.StrokeCounts;
                entry.Variants = entry.MiscGroup.Variants;
                entry.RadicalNames = entry.MiscGroup.RadicalNames;
                break;
            case QueryCodeGroup.XmlTagName:
                if (entry.QueryCodeGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.Character, QueryCodeGroup.XmlTagName);
                }
                entry.QueryCodeGroup = await _queryCodeGroupReader.ReadAsync(entry);
                entry.QueryCodes = entry.QueryCodeGroup.QueryCodes;
                break;
            case RadicalGroup.XmlTagName:
                if (entry.RadicalGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.Character, RadicalGroup.XmlTagName);
                }
                entry.RadicalGroup = await _radicalGroupReader.ReadAsync(entry);
                entry.Radicals = entry.RadicalGroup.Radicals;
                break;
            case ReadingMeaningGroup.XmlTagName:
                if (entry.ReadingMeaningGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.Character, ReadingMeaningGroup.XmlTagName);
                }
                entry.ReadingMeaningGroup = await _readingMeaningGroupReader.ReadAsync(entry);
                entry.Readings = entry.ReadingMeaningGroup.ReadingMeaning?.Readings ?? [];
                entry.Meanings = entry.ReadingMeaningGroup.ReadingMeaning?.Meanings ?? [];
                entry.IsKokuji = entry.ReadingMeaningGroup.ReadingMeaning?.IsKokuji ?? false;
                entry.IsGhost = entry.ReadingMeaningGroup.ReadingMeaning?.IsGhost ?? false;
                entry.Nanoris = entry.ReadingMeaningGroup.Nanoris;
                break;
            default:
                Log.UnexpectedChildElement(_logger, entry.Character, _xmlReader.Name, Entry.XmlTagName);
                entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadCharacterAsync(Entry entry)
    {
        if (entry.Character != string.Empty)
        {
            LogUnexpectedGroup(entry.Character, Entry.Character_XmlTagName);
        }

        entry.Character = await _xmlReader.ReadElementContentAsStringAsync();

        if (string.IsNullOrWhiteSpace(entry.Character))
        {
            LogMissingCharacter(Entry.Character_XmlTagName);
        }
        else if (entry.Character.EnumerateRunes().Count() > 1)
        {
            LogMultipleCharacters(Entry.Character_XmlTagName, entry.Character);
        }
    }

    [LoggerMessage(LogLevel.Error,
    "Entry contains no text in <{XmlTagName}> child element")]
    private partial void LogMissingCharacter(string xmlTagName);

    [LoggerMessage(LogLevel.Warning,
    "Entry contains more than one character in <{XmlTagName}> child element: `{Text}`")]
    private partial void LogMultipleCharacters(string xmlTagName, string text);

    [LoggerMessage(LogLevel.Warning,
    "Entry for character `{Character}` has more than one <{XmlTagName}> child element")]
    private partial void LogUnexpectedGroup(string character, string xmlTagName);
}
