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

internal class EntryReader
{
    private readonly XmlReader _xmlReader;
    private readonly CodepointGroupReader _codepointGroupReader;
    private readonly DictionaryGroupReader _dictionaryGroupReader;
    private readonly MiscGroupReader _miscGroupReader;
    private readonly QueryCodeGroupReader _queryCodeGroupReader;
    private readonly RadicalGroupReader _radicalGroupReader;
    private readonly ReadingMeaningGroupReader _readingMeaningGroupReader;
    private readonly ILogger<EntryReader> _logger;

    public EntryReader(XmlReader xmlReader, CodepointGroupReader codepointGroupReader, DictionaryGroupReader dictionaryGroupReader, MiscGroupReader miscGroupReader, QueryCodeGroupReader queryCodeGroupReader, RadicalGroupReader radicalGroupReader, ReadingMeaningGroupReader readingMeaningGroupReader, ILogger<EntryReader> logger) =>
        (_xmlReader, _codepointGroupReader, _dictionaryGroupReader, _miscGroupReader, _queryCodeGroupReader, _radicalGroupReader, _readingMeaningGroupReader, _logger) =
        (@xmlReader, @codepointGroupReader, @dictionaryGroupReader, @miscGroupReader, @queryCodeGroupReader, @radicalGroupReader, @readingMeaningGroupReader, @logger);

    public async Task<Entry> ReadAsync()
    {
        var entry = new Entry
        {
            Character = string.Empty,
            IsKokuji = false,
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
                    throw new Exception($"Unexpected text node found in `{Entry.XmlTagName}`: `{text}`");
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
            case "literal":
                entry.Character = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            case CodepointGroup.XmlTagName:
                if (entry.CodepointGroup is not null)
                {
                    throw new Exception($"Character {entry.Character} has more than one codepoint group.");
                }
                entry.CodepointGroup = await _codepointGroupReader.ReadAsync(entry);
                entry.Codepoints = entry.CodepointGroup.Codepoints;
                break;
            case DictionaryGroup.XmlTagName:
                if (entry.DictionaryGroup is not null)
                {
                    throw new Exception($"Character {entry.Character} has more than one dictionary group.");
                }
                entry.DictionaryGroup = await _dictionaryGroupReader.ReadAsync(entry);
                entry.Dictionaries = entry.DictionaryGroup.Dictionaries;
                break;
            case MiscGroup.XmlTagName:
                if (entry.MiscGroup is not null)
                {
                    throw new Exception($"Character {entry.Character} has more than one misc group.");
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
                    throw new Exception($"Character {entry.Character} has more than one query code group.");
                }
                entry.QueryCodeGroup = await _queryCodeGroupReader.ReadAsync(entry);
                entry.QueryCodes = entry.QueryCodeGroup.QueryCodes;
                break;
            case RadicalGroup.XmlTagName:
                if (entry.RadicalGroup is not null)
                {
                    throw new Exception($"Character {entry.Character} has more than one radical group.");
                }
                entry.RadicalGroup = await _radicalGroupReader.ReadAsync(entry);
                entry.Radicals = entry.RadicalGroup.Radicals;
                break;
            case ReadingMeaningGroup.XmlTagName:
                if (entry.ReadingMeaningGroup is not null)
                {
                    throw new Exception($"Character {entry.Character} has more than one reading/meaning group.");
                }
                entry.ReadingMeaningGroup = await _readingMeaningGroupReader.ReadAsync(entry);
                entry.Readings = entry.ReadingMeaningGroup.ReadingMeaning?.Readings ?? [];
                entry.Meanings = entry.ReadingMeaningGroup.ReadingMeaning?.Meanings ?? [];
                entry.IsKokuji = entry.ReadingMeaningGroup.ReadingMeaning?.IsKokuji ?? false;
                entry.Nanoris = entry.ReadingMeaningGroup.Nanoris;
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{_xmlReader.Name}` found in element `{Entry.XmlTagName}`");
        }
    }
}
