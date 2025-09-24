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

namespace Jitendex.Import.Jmdict.Readers;

internal class EntriesReader
{
    private readonly ILogger<EntriesReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly EntryReader _entryReader;
    private readonly ReferenceSequencer _referenceSequencer;

    public EntriesReader(ILogger<EntriesReader> logger, XmlReader xmlReader, EntryReader entryReader, ReferenceSequencer referenceSequencer) =>
        (_logger, _xmlReader, _entryReader, _referenceSequencer) =
        (@logger, @xmlReader, @entryReader, @referenceSequencer);

    public async Task<List<Entry>> ReadAsync()
    {
        var entries = new List<Entry>();

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
                        await _entryReader.ReadAsync(entries);
                    }
                    break;
            }
        }

        // Post-processing of all entries.
        _referenceSequencer.FixCrossReferences(entries);

        return entries;
    }
}
