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
using Jitendex.JMdict.Import.Models.EntryElements.SenseElements;
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

    public SenseReader(ILogger<SenseReader> logger, XmlReader xmlReader, KanjiFormRestrictionReader kRestrictionReader, ReadingRestrictionReader rRestrictionReader, CrossReferenceReader crossReferenceReader, DialectReader dialectReader, FieldReader fieldReader, GlossReader glossReader, LanguageSourceReader languageSourceReader, MiscReader miscReader, PartOfSpeechReader partOfSpeechReader)
        : base(logger, xmlReader) =>
        (_kRestrictionReader, _rRestrictionReader, _crossReferenceReader, _dialectReader, _fieldReader, _glossReader, _languageSourceReader, _miscReader, _partOfSpeechReader) =
        (@kRestrictionReader, @rRestrictionReader, @crossReferenceReader, @dialectReader, @fieldReader, @glossReader, @languageSourceReader, @miscReader, @partOfSpeechReader);

    public async Task ReadAsync(Document document, EntryElement entry)
    {
        var sense = new SenseElement
        {
            EntryId = entry.Id,
            Order = document.Senses.NextOrder(entry.Id),
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, sense);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    UnexpectedTextNode(SenseElement.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == SenseElement.XmlTagName;
                    break;
            }
        }

        document.Senses.Add(sense.Key(), sense);
    }

    private async Task ReadChildElementAsync(Document document, SenseElement sense)
    {
        switch (_xmlReader.Name)
        {
            case KanjiFormRestrictionElement.XmlTagName:
                await _kRestrictionReader.ReadAsync(document, sense);
                break;
            case ReadingRestrictionElement.XmlTagName:
                await _rRestrictionReader.ReadAsync(document, sense);
                break;
            case GlossElement.XmlTagName:
                await _glossReader.ReadAsync(document, sense);
                break;
            case PartOfSpeechElement.XmlTagName:
                await _partOfSpeechReader.ReadAsync(document, sense);
                break;
            case FieldElement.XmlTagName:
                await _fieldReader.ReadAsync(document, sense);
                break;
            case MiscElement.XmlTagName:
                await _miscReader.ReadAsync(document, sense);
                break;
            case DialectElement.XmlTagName:
                await _dialectReader.ReadAsync(document, sense);
                break;
            case LanguageSourceElement.XmlTagName:
                await _languageSourceReader.ReadAsync(document, sense);
                break;
            case CrossReferenceElement.XmlTagName:
            case CrossReferenceElement.XmlTagName_Antonym:
                await _crossReferenceReader.ReadAsync(document, sense);
                break;
            case SenseElement.Note_XmlTagName:
                await ReadSenseNote(sense);
                break;
            case "example":
                await _xmlReader.SkipAsync();
                break;
            default:
                UnexpectedChildElement(_xmlReader.Name, SenseElement.XmlTagName);
                break;
        }
    }

    private async Task ReadSenseNote(SenseElement sense)
    {
        // The XML schema allows for more than one note per sense,
        // but in practice there is only one or none.
        if (sense.Note != null)
        {
            LogTooManySenseNotes(sense.EntryId, sense.Order);
        }
        sense.Note = await _xmlReader.ReadElementContentAsStringAsync();
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} contains multiple sense notes")]
    partial void LogTooManySenseNotes(int entryId, int senseOrder);
}
