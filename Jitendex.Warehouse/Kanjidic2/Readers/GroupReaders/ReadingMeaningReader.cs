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

internal partial class ReadingMeaningReader
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
                    Log.UnexpectedTextNode(_logger, group.Entry.Character, ReadingMeaning.XmlTagName, text);
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
                await ReadReading(readingMeaning);
                break;
            case Meaning.XmlTagName:
                await ReadMeaning(readingMeaning);
                break;
            default:
                Log.UnexpectedChildElement(_logger, readingMeaning.Entry.Character, _xmlReader.Name, ReadingMeaning.XmlTagName);
                readingMeaning.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadReading(ReadingMeaning readingMeaning)
    {
        var typeName = _xmlReader.GetAttribute("r_type");
        if (string.IsNullOrWhiteSpace(typeName))
        {
            LogMissingTypeName(readingMeaning.Character);
            readingMeaning.Entry.IsCorrupt = true;
            typeName = string.Empty;
        }
        var reading = new Reading
        {
            Character = readingMeaning.Character,
            Order = readingMeaning.Readings.Count + 1,
            TypeName = typeName,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
            Entry = readingMeaning.Entry,
        };
        readingMeaning.Readings.Add(reading);
    }

    private async Task ReadMeaning(ReadingMeaning readingMeaning)
    {
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
            return; // Only want English meanings
        }
        if (meaning.Text == "(kokuji)")
        {
            readingMeaning.IsKokuji = true;
            return; // Don't add these "meanings"
        }
        if (meaning.Text == "(ghost kanji)")
        {
            readingMeaning.IsGhost = true;
            return; // Don't add these "meanings"
        }
        readingMeaning.Meanings.Add(meaning);
    }

    [LoggerMessage(LogLevel.Warning,
    "Character `{Character}` is missing a reading type attribute")]
    private partial void LogMissingTypeName(string character);
}
