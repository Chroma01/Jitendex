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

using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.JMdict.Import.Models;
using Jitendex.JMdict.Import.Models.EntryElements;
using Jitendex.JMdict.Import.Parsing.EntryElementReaders;

namespace Jitendex.JMdict.Import.Parsing;

internal partial class EntryReader : BaseReader<EntryReader>
{
    private readonly KanjiFormReader _kanjiFormReader;
    private readonly ReadingReader _readingReader;
    private readonly SenseReader _senseReader;

    public EntryReader(ILogger<EntryReader> logger, XmlReader xmlReader, KanjiFormReader kanjiFormReader, ReadingReader readingReader, SenseReader senseReader)
        : base(logger, xmlReader)
    {
        _kanjiFormReader = kanjiFormReader;
        _readingReader = readingReader;
        _senseReader = senseReader;
    }

    public async Task ReadAsync(Document document)
    {
        var entry = new Entry
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
                    var text = await _xmlReader.GetValueAsync();
                    UnexpectedTextNode(Entry.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Entry.XmlTagName;
                    break;
            }
        }

        if (entry.Id.Equals(default))
        {
            LogMissingEntryId(Entry.Id_XmlTagName);
        }
        else if (entry.IsJmdictEntry())
        {
            document.Entries.Add(entry.Id, entry);
        }
    }

    private async Task ReadChildElementAsync(Document document, Entry entry)
    {
        if (entry.Id.Equals(default))
        {
            if (string.Equals(_xmlReader.Name, Entry.Id_XmlTagName, StringComparison.Ordinal))
            {
                await ReadEntryId(entry);
            }
            else
            {
                LogPrematureElement(_xmlReader.Name);
            }
            return;
        }
        else if (!entry.IsJmdictEntry())
        {
            await _xmlReader.SkipAsync();
            return;
        }

        switch (_xmlReader.Name)
        {
            case KanjiForm.XmlTagName:
                await _kanjiFormReader.ReadAsync(document, entry);
                break;
            case Reading.XmlTagName:
                await _readingReader.ReadAsync(document, entry);
                break;
            case Sense.XmlTagName:
                await _senseReader.ReadAsync(document, entry);
                break;
            default:
                UnexpectedChildElement(_xmlReader.Name, Entry.XmlTagName);
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
        }
    }

    [LoggerMessage(LogLevel.Error,
    "Attempted to read <{XmlTagName}> child element before reading the entry primary key")]
    partial void LogPrematureElement(string xmlTagName);

    [LoggerMessage(LogLevel.Error,
    "Cannot parse entry ID from text `{Text}`")]
    partial void LogUnparsableId(string Text);

    [LoggerMessage(LogLevel.Error,
    "Entry contains no <{XmlTagName}> element; no primary key can be assigned")]
    partial void LogMissingEntryId(string xmlTagName);
}
