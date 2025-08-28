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
using Jitendex.Warehouse.Kanjidic2.Models.Groups;
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Readers.GroupReaders;

internal class ReadingMeaningReader
{
    private readonly ILogger<ReadingMeaningReader> _logger;
    private readonly XmlReader _xmlReader;

    public ReadingMeaningReader(ILogger<ReadingMeaningReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task<ReadingMeaning> ReadAsync(ReadingMeaningGroup group)
    {
        var readingMeaning = new ReadingMeaning
        {
            Character = group.Character,
            Entry = group.Entry,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(readingMeaning);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, ReadingMeaning.XmlTagName, text);
                    group.Entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == ReadingMeaning.XmlTagName;
                    break;
            }
        }
        return readingMeaning;
    }

    private async Task ReadChildElementAsync(ReadingMeaning readingMeaning)
    {
        switch (_xmlReader.Name)
        {
            case Reading.XmlTagName:
                readingMeaning.Readings.Add(new Reading
                {
                    Character = readingMeaning.Character,
                    Order = readingMeaning.Readings.Count + 1,
                    Type = _xmlReader.GetAttribute("r_type") ?? string.Empty,
                    Text = await _xmlReader.ReadElementContentAsStringAsync(),
                    Entry = readingMeaning.Entry,
                });
                break;
            case Meaning.XmlTagName:
                var meaning = new Meaning
                {
                    Character = readingMeaning.Character,
                    Order = readingMeaning.Meanings.Count + 1,
                    Language = _xmlReader.GetAttribute("m_lang") ?? "en",
                    Text = await _xmlReader.ReadElementContentAsStringAsync(),
                    Entry = readingMeaning.Entry,
                };
                if (meaning.Language != "en")
                {
                    break; // Only want English meanings
                }
                if (meaning.Text == "(kokuji)")
                {
                    readingMeaning.IsKokuji = true;
                    break; // Don't add these "meanings"
                }
                readingMeaning.Meanings.Add(meaning);
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, ReadingMeaning.XmlTagName);
                readingMeaning.Entry.IsCorrupt = true;
                return;
        }
    }
}
