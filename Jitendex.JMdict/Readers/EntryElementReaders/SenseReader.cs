/*
Copyright (c) 2025 Stephen Kraus
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
using Jitendex.JMdict.Models;
using Jitendex.JMdict.Models.EntryElements;
using Jitendex.JMdict.Models.EntryElements.SenseElements;
using Jitendex.JMdict.Readers.EntryElementReaders.SenseElementReaders;

namespace Jitendex.JMdict.Readers.EntryElementReaders;

internal partial class SenseReader
{
    private readonly ILogger<SenseReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KanjiFormRestrictionReader _kRestrictionReader;
    private readonly ReadingRestrictionReader _rRestrictionReader;
    private readonly CrossReferenceReader _crossReferenceReader;
    private readonly DialectReader _dialectReader;
    private readonly ExampleReader _exampleReader;
    private readonly FieldReader _fieldReader;
    private readonly GlossReader _glossReader;
    private readonly LanguageSourceReader _languageSourceReader;
    private readonly MiscReader _miscReader;
    private readonly PartOfSpeechReader _partOfSpeechReader;

    public SenseReader(ILogger<SenseReader> logger, XmlReader xmlReader, KanjiFormRestrictionReader kRestrictionReader, ReadingRestrictionReader rRestrictionReader, CrossReferenceReader crossReferenceReader, DialectReader dialectReader, ExampleReader exampleReader, FieldReader fieldReader, GlossReader glossReader, LanguageSourceReader languageSourceReader, MiscReader miscReader, PartOfSpeechReader partOfSpeechReader) =>
        (_logger, _xmlReader, _kRestrictionReader, _rRestrictionReader, _crossReferenceReader, _dialectReader, _exampleReader, _fieldReader, _glossReader, _languageSourceReader, _miscReader, _partOfSpeechReader) =
        (@logger, @xmlReader, @kRestrictionReader, @rRestrictionReader, @crossReferenceReader, @dialectReader, @exampleReader, @fieldReader, @glossReader, @languageSourceReader, @miscReader, @partOfSpeechReader);

    public async Task ReadAsync(Entry entry)
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

        if (sense.Glosses.Any(g => g.Language == "eng"))
        {
            entry.Senses.Add(sense);
        }
    }

    private async Task ReadChildElementAsync(Sense sense)
    {
        switch (_xmlReader.Name)
        {
            case KanjiFormRestriction.XmlTagName:
                await _kRestrictionReader.ReadAsync(sense);
                break;
            case ReadingRestriction.XmlTagName:
                await _rRestrictionReader.ReadAsync(sense);
                break;
            case Gloss.XmlTagName:
                await _glossReader.ReadAsync(sense);
                break;
            case PartOfSpeech.XmlTagName:
                await _partOfSpeechReader.ReadAsync(sense);
                break;
            case Field.XmlTagName:
                await _fieldReader.ReadAsync(sense);
                break;
            case Misc.XmlTagName:
                await _miscReader.ReadAsync(sense);
                break;
            case Dialect.XmlTagName:
                await _dialectReader.ReadAsync(sense);
                break;
            case LanguageSource.XmlTagName:
                await _languageSourceReader.ReadAsync(sense);
                break;
            case Example.XmlTagName:
                await _exampleReader.ReadAsync(sense);
                break;
            case RawCrossReference.XmlTagName:
            case RawCrossReference.XmlTagName_Antonym:
                await _crossReferenceReader.ReadAsync(sense);
                break;
            case Sense.Note_XmlTagName:
                await ReadSenseNote(sense);
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, Sense.XmlTagName);
                sense.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadSenseNote(Sense sense)
    {
        // The XML schema allows for more than one note per sense,
        // but in practice there is only one or none.
        if (sense.Note != null)
        {
            LogTooManySenseNotes(sense.EntryId, sense.Order);
            sense.Entry.IsCorrupt = true;
        }
        sense.Note = await _xmlReader.ReadElementContentAsStringAsync();
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} contains multiple sense notes")]
    partial void LogTooManySenseNotes(int entryId, int senseOrder);
}
