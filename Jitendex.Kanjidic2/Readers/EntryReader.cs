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
using Jitendex.Kanjidic2.Readers.GroupReaders;

namespace Jitendex.Kanjidic2.Readers;

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

    public async Task ReadAsync(List<Entry> entries)
    {
        var entry = new Entry
        {
            UnicodeScalarValue = default,
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
                    Log.UnexpectedTextNode(_logger, entry.ToRune(), Entry.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Entry.XmlTagName;
                    break;
            }
        }

        if (entry.UnicodeScalarValue != default)
        {
            entries.Add(entry);
        }
        else
        {
            LogMissingCharacter(Entry.Character_XmlTagName);
        }
    }

    private async Task ReadChildElementAsync(Entry entry)
    {
        if (_xmlReader.Name != Entry.Character_XmlTagName && entry.UnicodeScalarValue == default)
        {
            entry.IsCorrupt = true;
            LogPrematureElement(_xmlReader.Name);
            return;
        }

        switch (_xmlReader.Name)
        {
            case Entry.Character_XmlTagName:
                await ReadCharacterAsync(entry);
                break;
            case CodepointGroup.XmlTagName:
                if (entry.CodepointGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.ToRune(), CodepointGroup.XmlTagName);
                }
                entry.CodepointGroup = await _codepointGroupReader.ReadAsync(entry);
                entry.Codepoints = entry.CodepointGroup.Codepoints;
                break;
            case DictionaryGroup.XmlTagName:
                if (entry.DictionaryGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.ToRune(), DictionaryGroup.XmlTagName);
                }
                entry.DictionaryGroup = await _dictionaryGroupReader.ReadAsync(entry);
                entry.Dictionaries = entry.DictionaryGroup.Dictionaries;
                break;
            case MiscGroup.XmlTagName:
                if (entry.MiscGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.ToRune(), MiscGroup.XmlTagName);
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
                    LogUnexpectedGroup(entry.ToRune(), QueryCodeGroup.XmlTagName);
                }
                entry.QueryCodeGroup = await _queryCodeGroupReader.ReadAsync(entry);
                entry.QueryCodes = entry.QueryCodeGroup.QueryCodes;
                break;
            case RadicalGroup.XmlTagName:
                if (entry.RadicalGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.ToRune(), RadicalGroup.XmlTagName);
                }
                entry.RadicalGroup = await _radicalGroupReader.ReadAsync(entry);
                entry.Radicals = entry.RadicalGroup.Radicals;
                break;
            case ReadingMeaningGroup.XmlTagName:
                if (entry.ReadingMeaningGroup is not null)
                {
                    entry.IsCorrupt = true;
                    LogUnexpectedGroup(entry.ToRune(), ReadingMeaningGroup.XmlTagName);
                }
                entry.ReadingMeaningGroup = await _readingMeaningGroupReader.ReadAsync(entry);
                entry.Readings = entry.ReadingMeaningGroup.ReadingMeaning?.Readings ?? [];
                entry.Meanings = entry.ReadingMeaningGroup.ReadingMeaning?.Meanings ?? [];
                entry.IsKokuji = entry.ReadingMeaningGroup.ReadingMeaning?.IsKokuji ?? false;
                entry.IsGhost = entry.ReadingMeaningGroup.ReadingMeaning?.IsGhost ?? false;
                entry.Nanoris = entry.ReadingMeaningGroup.Nanoris;
                break;
            default:
                Log.UnexpectedChildElement(_logger, entry.ToRune(), _xmlReader.Name, Entry.XmlTagName);
                entry.IsCorrupt = true;
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

        var runes = text.EnumerateRunes();

        if (runes.Count() > 1)
        {
            LogMultipleCharacters(Entry.Character_XmlTagName, text);
        }

        entry.UnicodeScalarValue = runes.First().Value;
    }

#pragma warning disable IDE0060

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

#pragma warning restore IDE0060

}
