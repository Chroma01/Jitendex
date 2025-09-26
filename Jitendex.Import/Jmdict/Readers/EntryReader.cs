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
using Jitendex.Import.Jmdict.Models;
using Jitendex.Import.Jmdict.Models.EntryElements;
using Jitendex.Import.Jmdict.Readers.DocumentTypes;
using Jitendex.Import.Jmdict.Readers.EntryElementReaders;

namespace Jitendex.Import.Jmdict.Readers;

internal partial class EntryReader
{
    private readonly ILogger<EntryReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly CorpusCache _corpusCache;
    private readonly KanjiFormReader _kanjiFormReader;
    private readonly ReadingReader _readingReader;
    private readonly SenseReader _senseReader;

    public EntryReader(ILogger<EntryReader> logger, XmlReader xmlReader, CorpusCache corpusCache, KanjiFormReader kanjiFormReader, ReadingReader readingReader, SenseReader senseReader) =>
        (_logger, _xmlReader, _corpusCache, _kanjiFormReader, _readingReader, _senseReader) =
        (@logger, @xmlReader, @corpusCache, @kanjiFormReader, @readingReader, @senseReader);

    public async Task ReadAsync(List<Entry> entries)
    {
        var entry = new Entry
        {
            Id = default,
            CorpusId = default,
            Corpus = null!,
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
                    Log.UnexpectedTextNode(_logger, Entry.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Entry.XmlTagName;
                    break;
            }
        }

        if (entry.Corpus is not null)
        {
            PostProcess(entry);
            entries.Add(entry);
        }
        else
        {
            LogMissingEntryId(Entry.Id_XmlTagName);
        }
    }

    private async Task ReadChildElementAsync(Entry entry)
    {
        if(entry.Corpus is null && _xmlReader.Name != Entry.Id_XmlTagName)
        {
            LogPrematureElement(_xmlReader.Name);
            entry.IsCorrupt = true;
            return;
        }

        switch (_xmlReader.Name)
        {
            case Entry.Id_XmlTagName:
                await ReadEntryId(entry);
                AssignCorpus(entry);
                break;
            case KanjiForm.XmlTagName:
                await _kanjiFormReader.ReadAsync(entry);
                break;
            case Reading.XmlTagName:
                await _readingReader.ReadAsync(entry);
                break;
            case Sense.XmlTagName:
                await _senseReader.ReadAsync(entry);
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, Entry.XmlTagName);
                entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadEntryId(Entry entry)
    {
        var idText = await _xmlReader.ReadElementContentAsStringAsync();

        if (int.TryParse(idText, out int id))
        {
            entry.Id = id;
        }
        else
        {
            LogUnparsableId(idText);
            entry.IsCorrupt = true;
        }
    }

    private void AssignCorpus(Entry entry)
    {
        if (entry.Corpus is not null)
        {
            LogDuplicateEntryId(entry.Id, Entry.Id_XmlTagName);
            entry.IsCorrupt = true;
        }

        var corpus = _corpusCache.GetCorpus(entry);

        if (corpus.Id == CorpusId.Unknown)
        {
            entry.IsCorrupt = true;
        }

        entry.Corpus = corpus;
        entry.CorpusId = corpus.Id;
    }

    private void PostProcess(Entry entry)
    {
        BridgeReadingsAndKanjiForms(entry);
        CheckForKanjiFormOrphans(entry);
    }

    private static void BridgeReadingsAndKanjiForms(Entry entry)
    {
        foreach (var reading in entry.Readings)
        {
            if (reading.NoKanji || reading.IsHidden())
            {
                continue;
            }

            foreach (var kanjiForm in entry.KanjiForms)
            {
                if (kanjiForm.IsHidden())
                    continue;
                if (reading.Restrictions.Count > 0 && !reading.Restrictions.Any(r => r.KanjiFormOrder == kanjiForm.Order))
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

    private void CheckForKanjiFormOrphans(Entry entry)
    {
        foreach (var kanjiForm in entry.KanjiForms.Where(static k => !k.IsHidden()))
        {
            if (kanjiForm.ReadingBridges.Count == 0)
            {
                LogOrphanKanjiForm(kanjiForm.EntryId, kanjiForm.Text);
                entry.IsCorrupt = true;

                var defaultReading = entry.Readings.Where(static r => !r.IsHidden()).First();
                var bridge = new ReadingKanjiFormBridge
                {
                    EntryId = entry.Id,
                    ReadingOrder = defaultReading.Order,
                    KanjiFormOrder = kanjiForm.Order,
                    Reading = defaultReading,
                    KanjiForm = kanjiForm,
                };

                defaultReading.KanjiFormBridges.Add(bridge);
                kanjiForm.ReadingBridges.Add(bridge);
            }
        }
    }

    [LoggerMessage(LogLevel.Error,
    "Attempted to read <{XmlTagName}> child element before reading the entry primary key")]
    private partial void LogPrematureElement(string xmlTagName);

    [LoggerMessage(LogLevel.Warning,
    "Kanji form `{KanjiFormText}` in entry {EntryId} has no associated readings")]
    private partial void LogOrphanKanjiForm(int entryId, string kanjiFormText);

    [LoggerMessage(LogLevel.Error,
    "Entry ID {EntryId} contains more than one <{TagName}> element")]
    private partial void LogDuplicateEntryId(int entryId, string tagName);

    [LoggerMessage(LogLevel.Error,
    "Cannot parse entry ID from text `{Text}`")]
    private partial void LogUnparsableId(string Text);

    [LoggerMessage(LogLevel.Error,
    "Entry contains no <{XmlTagName}> element; no primary key can be assigned")]
    private partial void LogMissingEntryId(string xmlTagName);
}
