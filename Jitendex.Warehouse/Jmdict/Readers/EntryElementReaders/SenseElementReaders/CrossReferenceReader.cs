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

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal class CrossReferenceReader : IJmdictReader<Sense, CrossReference?>
{
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;
    private readonly ILogger<CrossReferenceReader> _logger;

    public CrossReferenceReader(XmlReader xmlReader, DocumentTypes docTypes, ILogger<CrossReferenceReader> logger) =>
        (_xmlReader, _docTypes, _logger) =
        (@xmlReader, @docTypes, @logger);

    private record ParsedText(string Text1, string? Text2, int SenseOrder);

    public async Task<CrossReference?> ReadAsync(Sense sense)
    {
        var typeName = _xmlReader.Name;
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        if (sense.Entry.CorpusId != CorpusId.Jmdict)
        {
            // TODO: Log
            return null;
        }
        ParsedText parsedText;
        try
        {
            parsedText = Parse(text);
        }
        catch
        {
            // TODO: Log
            return null;
        }
        var type = _docTypes.GetKeywordByName<CrossReferenceType>(typeName);
        var crossRef = new CrossReference
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.CrossReferences.Count + 1,
            TypeName = typeName,
            Type = type,
            RefEntryId = -1,
            RefReadingOrder = -1,
            RefText1 = parsedText.Text1,
            RefText2 = parsedText.Text2,
            RefSenseOrder = parsedText.SenseOrder,
            Sense = sense,
        };
        return crossRef;
    }

    private static ParsedText Parse(string text)
    {
        const char separator = '・';
        var split = text.Split(separator);
        (string, string?, int) parsed;
        switch (split.Length)
        {
            case 1:
                parsed = (split[0], null, 1);
                break;
            case 2:
                if (int.TryParse(split[1], out int s1))
                    parsed = (split[0], null, s1);
                else
                    parsed = (split[0], split[1], 1);
                break;
            case 3:
                if (int.TryParse(split[2], out int s2))
                    parsed = (split[0], split[1], s2);
                else
                    throw new ArgumentException($"Third value in text `{text}` must be an integer", nameof(text));
                break;
            default:
                throw new ArgumentException($"Too many separator characters `{separator}` in text `{text}`", nameof(text));
        }
        return new ParsedText(parsed.Item1, parsed.Item2, parsed.Item3);
    }
}
