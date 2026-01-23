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

namespace Jitendex.JMdict.Import.Parsing.EntryElementReaders.SenseElementReaders;

internal partial class CrossReferenceReader : BaseReader<CrossReferenceReader>
{
   public CrossReferenceReader(ILogger<CrossReferenceReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    private record ParsedReference(string Text1, string? Text2, int SenseOrder);

    public async Task ReadAsync(Document document, SenseElement sense)
    {
        var typeName = _xmlReader.Name;
        if (!document.CrossReferenceTypes.ContainsKey(typeName))
        {
            var tag = new CrossReferenceTypeElement(typeName, document.Header.Date);
            document.CrossReferenceTypes.Add(typeName, tag);
        }

        var text = await _xmlReader.ReadElementContentAsStringAsync();
        var parsedRef = Parse(text);
        if (parsedRef is null)
        {
            return;
        }

        var xref = new CrossReferenceElement
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = document.NextOrder(sense.Key(), nameof(CrossReferenceElement)),
            TypeName = typeName,
            RefText1 = parsedRef.Text1,
            RefText2 = parsedRef.Text2,
            RefSenseOrder = parsedRef.SenseOrder,
        };

        document.CrossReferences.Add(xref.Key(), xref);
    }

    private ParsedReference? Parse(string text)
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
                if (int.TryParse(split[1], out int x))
                {
                    parsed = (split[0], null, x);
                }
                else
                {
                    parsed = (split[0], split[1], 1);
                }
                break;
            case 3:
                if (int.TryParse(split[2], out int y))
                {
                    parsed = (split[0], split[1], y);
                }
                else
                {
                    LogNonIntegerSenseOrder(text, split[2]);
                    return null;
                }
                break;
            default:
                LogTooManySeparators(text, separator);
                return null;
        }
        return new ParsedReference(parsed.Item1, parsed.Item2, parsed.Item3);
    }

    [LoggerMessage(LogLevel.Error,
    "Third value `{ThirdValue}` in reference text `{Text}` must be an integer")]
    partial void LogNonIntegerSenseOrder(string text, string thirdValue);

    [LoggerMessage(LogLevel.Error,
    "Too many separator characters `{Separator}` in reference text `{Text}`")]
    partial void LogTooManySeparators(string text, char separator);
}
