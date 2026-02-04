/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.JMdict.Import.Models;
using Jitendex.JMdict.Import.Models.EntryElements;
using Jitendex.JMdict.Import.Parsing.EntryElementReaders.SenseElementReaders;

namespace Jitendex.JMdict.Import.Parsing.EntryElementReaders;

internal partial class SenseReader : BaseReader<SenseReader>
{
    private readonly KanjiFormRestrictionReader _kRestrictionReader;
    private readonly ReadingRestrictionReader _rRestrictionReader;
    private readonly CrossReferenceReader _crossReferenceReader;
    private readonly DialectReader _dialectReader;
    private readonly FieldReader _fieldReader;
    private readonly GlossReader _glossReader;
    private readonly LanguageSourceReader _languageSourceReader;
    private readonly MiscReader _miscReader;
    private readonly PartOfSpeechReader _partOfSpeechReader;

    public SenseReader(ILogger<SenseReader> logger, KanjiFormRestrictionReader kRestrictionReader, ReadingRestrictionReader rRestrictionReader, CrossReferenceReader crossReferenceReader, DialectReader dialectReader, FieldReader fieldReader, GlossReader glossReader, LanguageSourceReader languageSourceReader, MiscReader miscReader, PartOfSpeechReader partOfSpeechReader)
        : base(logger) =>
        (_kRestrictionReader, _rRestrictionReader, _crossReferenceReader, _dialectReader, _fieldReader, _glossReader, _languageSourceReader, _miscReader, _partOfSpeechReader) =
        (@kRestrictionReader, @rRestrictionReader, @crossReferenceReader, @dialectReader, @fieldReader, @glossReader, @languageSourceReader, @miscReader, @partOfSpeechReader);

    public async Task ReadAsync(XmlReader xmlReader, Document document, EntryElement entry)
    {
        var sense = new SenseElement
        {
            EntryId = entry.Id,
            Order = document.Senses.NextOrder(entry.Id),
        };

        var exit = false;
        while (!exit && await xmlReader.ReadAsync())
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(xmlReader, document, sense);
                    break;
                case XmlNodeType.Text:
                    await LogUnexpectedTextNodeAsync(xmlReader, XmlTagName.Sense);
                    break;
                case XmlNodeType.EndElement:
                    exit = IsClosingTag(xmlReader, XmlTagName.Sense);
                    break;
            }
        }

        document.Senses.Add(sense.Key(), sense);
    }

    private async Task ReadChildElementAsync(XmlReader xmlReader, Document document, SenseElement sense)
    {
        switch (xmlReader.Name)
        {
            case XmlTagName.Gloss:
                await _glossReader.ReadAsync(xmlReader, document, sense);
                break;
            case XmlTagName.PartOfSpeech:
                await _partOfSpeechReader.ReadAsync(xmlReader, document, sense);
                break;
            case XmlTagName.Misc:
                await _miscReader.ReadAsync(xmlReader, document, sense);
                break;
            case XmlTagName.CrossReference:
            case XmlTagName.Antonym:
                await _crossReferenceReader.ReadAsync(xmlReader, document, sense);
                break;
            case XmlTagName.Example:
                await xmlReader.SkipAsync();
                break;
            case XmlTagName.Field:
                await _fieldReader.ReadAsync(xmlReader, document, sense);
                break;
            case XmlTagName.LanguageSource:
                await _languageSourceReader.ReadAsync(xmlReader, document, sense);
                break;
            case XmlTagName.SenseNote:
                await ReadSenseNote(xmlReader, sense);
                break;
            case XmlTagName.SenseReadingRestriction:
                await _rRestrictionReader.ReadAsync(xmlReader, document, sense);
                break;
            case XmlTagName.SenseKanjiFormRestriction:
                await _kRestrictionReader.ReadAsync(xmlReader, document, sense);
                break;
            case XmlTagName.Dialect:
                await _dialectReader.ReadAsync(xmlReader, document, sense);
                break;
            default:
                LogUnexpectedChildElement(xmlReader, XmlTagName.Sense);
                break;
        }
    }

    private async Task ReadSenseNote(XmlReader xmlReader, SenseElement sense)
    {
        // The XML schema allows for more than one note per sense,
        // but in practice there is only one or none.
        if (sense.Note != null)
        {
            LogTooManySenseNotes(sense.EntryId, sense.Order);
        }
        sense.Note = await xmlReader.ReadElementContentAsStringAsync();
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} contains multiple sense notes")]
    partial void LogTooManySenseNotes(int entryId, int senseOrder);
}
