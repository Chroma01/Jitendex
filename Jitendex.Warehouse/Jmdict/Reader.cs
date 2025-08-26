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

namespace Jitendex.Warehouse.Jmdict;

internal class Reader
{
    private readonly XmlReader _xmlReader;
    private readonly EntityFactory _factory;
    private readonly IJmdictReader<NoParent, NoChild> _documentReader;
    private readonly IJmdictReader<NoParent, Entry> _entryReader;
    private readonly ReferenceSequencer _referenceSequencer;
    private readonly ILogger<Reader> _logger;

    public Reader(XmlReader xmlReader, EntityFactory factory, IJmdictReader<NoParent, NoChild> documentReader, IJmdictReader<NoParent, Entry> entryReader, ReferenceSequencer referenceSequencer, ILogger<Reader> logger)
    {
        _xmlReader = xmlReader;
        _factory = factory;
        _documentReader = documentReader;
        _entryReader = entryReader;
        _referenceSequencer = referenceSequencer;
        _logger = logger;
    }

    public async Task<List<Entry>> ReadEntriesAsync()
    {
        await _documentReader.ReadAsync(new NoParent());

        var entries = new List<Entry>();
        await foreach (var entry in EnumerateEntriesAsync())
        {
            entries.Add(entry);
        }
        _referenceSequencer.FixCrossReferences(entries);

        return entries;
    }

    private async IAsyncEnumerable<Entry> EnumerateEntriesAsync()
    {
        var nothing = new NoParent();
        while (await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    _logger.LogError("Document type should have been read before any entries.");
                    break;
                case XmlNodeType.Element:
                    if (_xmlReader.Name == Entry.XmlTagName)
                    {
                        var entry = await _entryReader.ReadAsync(nothing);
                        yield return entry;
                    }
                    break;
            }
        }
    }
}
