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

internal class Service
{
    private readonly ILogger<Service> _logger;
    private readonly XmlReader _xmlReader;
    private readonly IJmdictReader<NoParent, NoChild> _documentReader;
    private readonly IJmdictReader<NoParent, Entry> _entryReader;
    private readonly ReferenceSequencer _referenceSequencer;

    public Service(ILogger<Service> logger, XmlReader xmlReader, IJmdictReader<NoParent, NoChild> documentReader, IJmdictReader<NoParent, Entry> entryReader, ReferenceSequencer referenceSequencer) =>
        (_logger, _xmlReader, _documentReader, _entryReader, _referenceSequencer) =
        (@logger, @xmlReader, @documentReader, @entryReader, @referenceSequencer);

    public async Task<List<Entry>> CreateEntriesAsync()
    {
        var entries = new List<Entry>();
        await foreach (var entry in EnumerateEntriesAsync())
        {
            entries.Add(entry);
        }

        // Post-processing of all entries.
        _referenceSequencer.FixCrossReferences(entries);

        return entries;
    }

    private async IAsyncEnumerable<Entry> EnumerateEntriesAsync()
    {
        var @void = new NoParent();
        await _documentReader.ReadAsync(@void);

        while (await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    _logger.LogWarning("Unexpected document type node `{Name}`", _xmlReader.Name);
                    break;
                case XmlNodeType.Element:
                    if (_xmlReader.Name == Entry.XmlTagName)
                    {
                        var entry = await _entryReader.ReadAsync(@void);
                        if (entry is not null)
                        {
                            yield return entry;
                        }
                    }
                    break;
            }
        }
    }
}
