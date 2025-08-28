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

internal partial class SenseReader : IJmdictReader<Entry, Sense>
{
    private readonly ILogger<SenseReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly IJmdictReader<Sense, CrossReference> _crossReferenceReader;
    private readonly IJmdictReader<Sense, Dialect> _dialectReader;
    private readonly IJmdictReader<Sense, Example?> _exampleReader;
    private readonly IJmdictReader<Sense, Field> _fieldReader;
    private readonly IJmdictReader<Sense, Gloss> _glossReader;
    private readonly IJmdictReader<Sense, LanguageSource> _languageSourceReader;
    private readonly IJmdictReader<Sense, Misc> _miscReader;
    private readonly IJmdictReader<Sense, PartOfSpeech> _partOfSpeechReader;

    public SenseReader(ILogger<SenseReader> logger, XmlReader xmlReader, IJmdictReader<Sense, CrossReference> crossReferenceReader, IJmdictReader<Sense, Dialect> dialectReader, IJmdictReader<Sense, Example?> exampleReader, IJmdictReader<Sense, Field> fieldReader, IJmdictReader<Sense, Gloss> glossReader, IJmdictReader<Sense, LanguageSource> languageSourceReader, IJmdictReader<Sense, Misc> miscReader, IJmdictReader<Sense, PartOfSpeech> partOfSpeechReader) =>
        (_logger, _xmlReader, _crossReferenceReader, _dialectReader, _exampleReader, _fieldReader, _glossReader, _languageSourceReader, _miscReader, _partOfSpeechReader) =
        (@logger, @xmlReader, @crossReferenceReader, @dialectReader, @exampleReader, @fieldReader, @glossReader, @languageSourceReader, @miscReader, @partOfSpeechReader);

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
                    Log.UnexpectedTextNode(_logger, Sense.XmlTagName, text);
                    sense.Entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Sense.XmlTagName;
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
                var kanjiFormText = await _xmlReader.ReadElementContentAsStringAsync();
                var kRestriction = new KanjiFormRestriction
                {
                    EntryId = sense.EntryId,
                    SenseOrder = sense.Order,
                    Order = sense.KanjiFormRestrictions.Count + 1,
                    KanjiFormOrder = -1,
                    Sense = sense,
                };
                var kanjiForm = sense.Entry.KanjiForms
                    .Where(k => k.Text == kanjiFormText)
                    .FirstOrDefault();
                if (kanjiForm is not null)
                {
                    kRestriction.KanjiFormOrder = kanjiForm.Order;
                    kRestriction.KanjiForm = kanjiForm;
                    sense.KanjiFormRestrictions.Add(kRestriction);
                }
                else
                {
                    LogInvalidKanjiFormRestriction(sense.EntryId, sense.Order, kanjiFormText);
                    sense.Entry.IsCorrupt = true;
                }
                break;
            case "stagr":
                var readingText = await _xmlReader.ReadElementContentAsStringAsync();
                var rRestriction = new ReadingRestriction
                {
                    EntryId = sense.EntryId,
                    SenseOrder = sense.Order,
                    Order = sense.ReadingRestrictions.Count + 1,
                    ReadingOrder = -1,
                    Sense = sense,
                };
                var reading = sense.Entry.Readings
                    .Where(r => r.Text == readingText)
                    .FirstOrDefault();
                if (reading is not null)
                {
                    rRestriction.ReadingOrder = reading.Order;
                    rRestriction.Reading = reading;
                    sense.ReadingRestrictions.Add(rRestriction);
                }
                else
                {
                    LogInvalidReadingRestriction(sense.EntryId, sense.Order, readingText);
                    sense.Entry.IsCorrupt = true;
                }
                break;
            case "s_inf":
                if (sense.Note != null)
                {
                    // The XML schema allows for more than one note per sense,
                    // but in practice there is only one or none.
                    LogTooManySenseNotes(sense.EntryId, sense.Order);
                    sense.Entry.IsCorrupt = true;
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
                    sense.CrossReferences.Add(reference);
                else
                    sense.Entry.IsCorrupt = true;
                break;
            case LanguageSource.XmlTagName:
                var languageSource = await _languageSourceReader.ReadAsync(sense);
                sense.LanguageSources.Add(languageSource);
                break;
            case Example.XmlTagName:
                var example = await _exampleReader.ReadAsync(sense);
                if (example is not null)
                    sense.Examples.Add(example);
                else
                    sense.Entry.IsCorrupt = true;
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, Sense.XmlTagName);
                sense.Entry.IsCorrupt = true;
                break;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} contains multiple sense notes")]
    private partial void LogTooManySenseNotes(int entryId, int senseOrder);

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} contains an invalid kanji form restriction: `{Text}`")]
    private partial void LogInvalidKanjiFormRestriction(int entryId, int senseOrder, string text);

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} contains an invalid reading restriction: `{Text}`")]
    private partial void LogInvalidReadingRestriction(int entryId, int senseOrder, string text);

}
