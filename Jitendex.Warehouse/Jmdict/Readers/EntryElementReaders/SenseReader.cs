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

using Jitendex.Warehouse.Jmdict.Models;
using Jitendex.Warehouse.Jmdict.Models.EntryElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;
using Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders;

internal class SenseReader
{
    private readonly XmlReader Reader;
    private readonly EntityFactory Factory;
    private readonly CrossReferenceReader CrossReferenceReader;
    private readonly DialectReader DialectReader;
    private readonly ExampleReader ExampleReader;
    private readonly FieldReader FieldReader;
    private readonly GlossReader GlossReader;
    private readonly LanguageSourceReader LanguageSourceReader;
    private readonly MiscReader MiscReader;
    private readonly PartOfSpeechReader PartOfSpeechReader;

    public SenseReader(XmlReader reader, EntityFactory factory)
    {
        Reader = reader;
        Factory = factory;
        CrossReferenceReader = new CrossReferenceReader(reader, factory);
        DialectReader = new DialectReader(reader, factory);
        ExampleReader = new ExampleReader(reader, factory);
        FieldReader = new FieldReader(reader, factory);
        GlossReader = new GlossReader(reader, factory);
        LanguageSourceReader = new LanguageSourceReader(reader, factory);
        MiscReader = new MiscReader(reader, factory);
        PartOfSpeechReader = new PartOfSpeechReader(reader, factory);
    }

    public async Task<Sense> ReadAsync(Entry entry)
    {
        var sense = new Sense
        {
            EntryId = entry.Id,
            Order = entry.Senses.Count + 1,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await Reader.ReadAsync())
        {
            switch (Reader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(sense);
                    break;
                case XmlNodeType.Text:
                    var text = await Reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{Sense.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = Reader.Name == Sense.XmlTagName;
                    break;
            }
        }

        return sense;
    }

    private async Task ReadChildElementAsync(Sense sense)
    {
        switch (Reader.Name)
        {
            case "stagk":
                var kanjiFormTextRestriction = await Reader.ReadElementContentAsStringAsync();
                sense.KanjiFormTextRestrictions.Add(kanjiFormTextRestriction);
                break;
            case "stagr":
                var readingTextRestriction = await Reader.ReadElementContentAsStringAsync();
                sense.ReadingTextRestrictions.Add(readingTextRestriction);
                break;
            case "s_inf":
                if (sense.Note != null)
                {
                    // The XML schema allows for more than one note per sense,
                    // but in practice there is only one or none.
                    // TODO: Log warning
                }
                sense.Note = await Reader.ReadElementContentAsStringAsync();
                break;
            case Gloss.XmlTagName:
                var gloss = await GlossReader.ReadAsync(sense);
                if (gloss.Language == "eng")
                {
                    sense.Glosses.Add(gloss);
                }
                break;
            case PartOfSpeech.XmlTagName:
                var pos = await PartOfSpeechReader.ReadAsync(sense);
                sense.PartsOfSpeech.Add(pos);
                break;
            case Field.XmlTagName:
                var field = await FieldReader.ReadAsync(sense);
                sense.Fields.Add(field);
                break;
            case Misc.XmlTagName:
                var misc = await MiscReader.ReadAsync(sense);
                sense.Miscs.Add(misc);
                break;
            case Dialect.XmlTagName:
                var dial = await DialectReader.ReadAsync(sense);
                sense.Dialects.Add(dial);
                break;
            case "xref":
            case "ant":
                var reference = await CrossReferenceReader.ReadAsync(sense);
                if (reference is not null)
                {
                    sense.CrossReferences.Add(reference);
                }
                break;
            case LanguageSource.XmlTagName:
                var languageSource = await LanguageSourceReader.ReadAsync(sense);
                sense.LanguageSources.Add(languageSource);
                break;
            case Example.XmlTagName:
                var example = await ExampleReader.ReadAsync(sense);
                sense.Examples.Add(example);
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{Reader.Name}` found in element `{Sense.XmlTagName}`");
        }
    }
}
