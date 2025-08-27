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
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders;

internal class SenseReader : IJmdictReader<Entry, Sense>
{
    private readonly XmlReader _xmlReader;
    private readonly IJmdictReader<Sense, CrossReference> _crossReferenceReader;
    private readonly IJmdictReader<Sense, Dialect> _dialectReader;
    private readonly IJmdictReader<Sense, Example> _exampleReader;
    private readonly IJmdictReader<Sense, Field> _fieldReader;
    private readonly IJmdictReader<Sense, Gloss> _glossReader;
    private readonly IJmdictReader<Sense, LanguageSource> _languageSourceReader;
    private readonly IJmdictReader<Sense, Misc> _miscReader;
    private readonly IJmdictReader<Sense, PartOfSpeech> _partOfSpeechReader;
    private readonly ILogger<SenseReader> _logger;

    public SenseReader(XmlReader xmlReader, IJmdictReader<Sense, CrossReference> crossReferenceReader, IJmdictReader<Sense, Dialect> dialectReader, IJmdictReader<Sense, Example> exampleReader, IJmdictReader<Sense, Field> fieldReader, IJmdictReader<Sense, Gloss> glossReader, IJmdictReader<Sense, LanguageSource> languageSourceReader, IJmdictReader<Sense, Misc> miscReader, IJmdictReader<Sense, PartOfSpeech> partOfSpeechReader, ILogger<SenseReader> logger) =>
        (_xmlReader, _crossReferenceReader, _dialectReader, _exampleReader, _fieldReader, _glossReader, _languageSourceReader, _miscReader, _partOfSpeechReader, _logger) =
        (@xmlReader, @crossReferenceReader, @dialectReader, @exampleReader, @fieldReader, @glossReader, @languageSourceReader, @miscReader, @partOfSpeechReader, @logger);

    public async Task<Sense> ReadAsync(Entry entry)
    {
        var sense = new Sense
        {
            EntryId = entry.Id,
            Order = entry.Senses.Count + 1,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(sense);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{Sense.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = true;
                    break;
            }
        }
        return sense;
    }

    private async Task ReadChildElementAsync(Sense sense)
    {
        switch (_xmlReader.Name)
        {
            case "stagk":
                var kanjiFormTextRestriction = await _xmlReader.ReadElementContentAsStringAsync();
                sense.KanjiFormTextRestrictions.Add(kanjiFormTextRestriction);
                break;
            case "stagr":
                var readingTextRestriction = await _xmlReader.ReadElementContentAsStringAsync();
                sense.ReadingTextRestrictions.Add(readingTextRestriction);
                break;
            case "s_inf":
                if (sense.Note != null)
                {
                    // The XML schema allows for more than one note per sense,
                    // but in practice there is only one or none.
                    // TODO: Log warning
                }
                sense.Note = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            case Gloss.XmlTagName:
                var gloss = await _glossReader.ReadAsync(sense);
                if (gloss.Language == "eng")
                {
                    sense.Glosses.Add(gloss);
                }
                break;
            case PartOfSpeech.XmlTagName:
                var pos = await _partOfSpeechReader.ReadAsync(sense);
                sense.PartsOfSpeech.Add(pos);
                break;
            case Field.XmlTagName:
                var field = await _fieldReader.ReadAsync(sense);
                sense.Fields.Add(field);
                break;
            case Misc.XmlTagName:
                var misc = await _miscReader.ReadAsync(sense);
                sense.Miscs.Add(misc);
                break;
            case Dialect.XmlTagName:
                var dial = await _dialectReader.ReadAsync(sense);
                sense.Dialects.Add(dial);
                break;
            case "xref":
            case "ant":
                var reference = await _crossReferenceReader.ReadAsync(sense);
                if (reference is not null)
                {
                    sense.CrossReferences.Add(reference);
                }
                break;
            case LanguageSource.XmlTagName:
                var languageSource = await _languageSourceReader.ReadAsync(sense);
                sense.LanguageSources.Add(languageSource);
                break;
            case Example.XmlTagName:
                var example = await _exampleReader.ReadAsync(sense);
                sense.Examples.Add(example);
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{_xmlReader.Name}` found in element `{Sense.XmlTagName}`");
        }
    }
}
