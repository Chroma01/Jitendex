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
using Jitendex.Warehouse.Jmdict.Models;
using Jitendex.Warehouse.Jmdict.Models.EntryElements;

namespace Jitendex.Warehouse.Jmdict.Readers;

internal class EntryReader : IJmdictReader<NoParent, Entry>
{
    private readonly ILogger<EntryReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;
    private readonly IJmdictReader<Entry, KanjiForm> _kanjiFormReader;
    private readonly IJmdictReader<Entry, Reading> _readingReader;
    private readonly IJmdictReader<Entry, Sense> _senseReader;

    public EntryReader(ILogger<EntryReader> logger, XmlReader xmlReader, DocumentTypes docTypes, IJmdictReader<Entry, KanjiForm> kanjiFormReader, IJmdictReader<Entry, Reading> readingReader, IJmdictReader<Entry, Sense> senseReader) =>
        (_logger, _xmlReader, _docTypes, _kanjiFormReader, _readingReader, _senseReader) =
        (@logger, @xmlReader, @docTypes, @kanjiFormReader, @readingReader, @senseReader);

    public async Task<Entry> ReadAsync(NoParent noParent)
    {
        var entry = new Entry
        {
            Id = -1,
            CorpusId = CorpusId.Unknown,
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
        return PostProcess(entry);
    }

    private async Task ReadChildElementAsync(Entry entry)
    {
        switch (_xmlReader.Name)
        {
            case "ent_seq":
                var sequence = await _xmlReader.ReadElementContentAsStringAsync();
                entry.Id = int.Parse(sequence);
                entry.Corpus = _docTypes.GetCorpus(entry.Id);
                entry.CorpusId = entry.Corpus.Id;
                break;
            case KanjiForm.XmlTagName:
                var kanjiForm = await _kanjiFormReader.ReadAsync(entry);
                entry.KanjiForms.Add(kanjiForm);
                break;
            case Reading.XmlTagName:
                var reading = await _readingReader.ReadAsync(entry);
                entry.Readings.Add(reading);
                break;
            case Sense.XmlTagName:
                var sense = await _senseReader.ReadAsync(entry);
                if (sense.Glosses.Any(g => g.Language == "eng"))
                {
                    entry.Senses.Add(sense);
                }
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{_xmlReader.Name}` found in element `{Entry.XmlTagName}`");
        }
    }

    private static Entry PostProcess(Entry entry)
    {
        BridgeReadingsAndKanjiForms(entry);
        // Anticipating more operations here later.
        return entry;
    }

    private static void BridgeReadingsAndKanjiForms(Entry entry)
    {
        foreach (var reading in entry.Readings)
        {
            if (reading.NoKanji)
                continue;
            if (reading.IsHidden())
                continue;

            foreach (var kanjiForm in entry.KanjiForms)
            {
                if (kanjiForm.IsHidden())
                    continue;
                if (reading.ConstraintKanjiFormTexts.Count > 0 && !reading.ConstraintKanjiFormTexts.Contains(kanjiForm.Text))
                    continue;
                var bridge = new ReadingKanjiFormBridge
                {
                    EntryId = entry.Id,
                    ReadingOrder = reading.Order,
                    KanjiFormOrder = kanjiForm.Order,
                    Reading = reading,
                    KanjiForm = kanjiForm,
                };
                reading.KanjiFormBridges.Add(bridge);
                kanjiForm.ReadingBridges.Add(bridge);
            }
        }
    }
}
