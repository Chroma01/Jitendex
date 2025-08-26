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
using Jitendex.Warehouse.Jmdict.Readers;

namespace Jitendex.Warehouse.Jmdict;

internal class Reader
{
    private readonly XmlReader _xmlReader;
    private readonly EntityFactory _factory;
    private readonly EntryReader _entryReader;
    private readonly ReferenceSequencer _referenceSequencer;
    private readonly ILogger<Reader> _logger;

    public Reader(XmlReader xmlReader, EntityFactory factory, EntryReader entryReader, ReferenceSequencer referenceSequencer, ILogger<Reader> logger)
    {
        _xmlReader = xmlReader;
        _factory = factory;
        _entryReader = entryReader;
        _referenceSequencer = referenceSequencer;
        _logger = logger;
    }

    public async Task<List<Entry>> ReadEntriesAsync()
    {
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
        while (await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    var dtd = await _xmlReader.GetValueAsync();
                    RegisterFactoryKeywords(dtd);
                    break;
                case XmlNodeType.Element:
                    if (_xmlReader.Name == Entry.XmlTagName)
                    {
                        var entry = await _entryReader.ReadAsync();
                        yield return entry;
                    }
                    break;
            }
        }
    }

    private void RegisterFactoryKeywords(string dtd)
    {
        // Entities explicitly defined in document header.
        var nameToDescription = DocumentTypeDefinition.ParseEntities(dtd);
        foreach (var (name, description) in nameToDescription)
        {
            // Since there's no keyword overlap between these types,
            // it's fine to register all the definitions for all of the types.
            _factory.RegisterKeyword<ReadingInfoTag>(name, description);
            _factory.RegisterKeyword<KanjiFormInfoTag>(name, description);
            _factory.RegisterKeyword<PartOfSpeechTag>(name, description);
            _factory.RegisterKeyword<FieldTag>(name, description);
            _factory.RegisterKeyword<MiscTag>(name, description);
            _factory.RegisterKeyword<DialectTag>(name, description);
        }

        // Entities implicitly defined that cannot be parsed from the document.
        foreach (var (name, description) in GlossType.NameToDescription)
            _factory.RegisterKeyword<GlossType>(name, description);

        foreach (var (name, description) in CrossReferenceType.NameToDescription)
            _factory.RegisterKeyword<CrossReferenceType>(name, description);

        foreach (var (name, description) in LanguageSourceType.NameToDescription)
            _factory.RegisterKeyword<LanguageSourceType>(name, description);

        foreach (var (name, description) in ExampleSourceType.NameToDescription)
            _factory.RegisterKeyword<ExampleSourceType>(name, description);

        foreach (var (name, description) in PriorityTag.NameToDescription)
            _factory.RegisterKeyword<PriorityTag>(name, description);
    }
}
