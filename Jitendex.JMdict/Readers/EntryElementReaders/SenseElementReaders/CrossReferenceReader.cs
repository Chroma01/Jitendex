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
using Jitendex.JMdict.Models;
using Jitendex.JMdict.Models.EntryElements;
using Jitendex.JMdict.Models.EntryElements.SenseElements;
using Jitendex.JMdict.Readers.DocumentTypes;

namespace Jitendex.JMdict.Readers.EntryElementReaders.SenseElementReaders;

internal partial class CrossReferenceReader
{
    private readonly ILogger<CrossReferenceReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KeywordCache _keywordCache;

    public CrossReferenceReader(ILogger<CrossReferenceReader> logger, XmlReader xmlReader, KeywordCache keywordCache) =>
        (_logger, _xmlReader, _keywordCache) =
        (@logger, @xmlReader, @keywordCache);

    private record ParsedReference(string Text1, string? Text2, int SenseOrder);

    public async Task ReadAsync(Sense sense)
    {
        var typeName = _xmlReader.Name;
        var text = await _xmlReader.ReadElementContentAsStringAsync();

        if (sense.Entry.CorpusId != CorpusId.Jmdict)
        {
            LogUnsupportedCorpus(sense.Entry.Id, sense.Entry.Corpus.Name);
            sense.Entry.IsCorrupt = true;
            return;
        }

        var parsedRef = Parse(text);

        if (parsedRef is null)
        {
            sense.Entry.IsCorrupt = true;
            return;
        }

        var type = _keywordCache.GetByName<CrossReferenceType>(typeName);

        if (type.IsCorrupt)
        {
            sense.Entry.IsCorrupt = true;
        }

        var crossRef = new RawCrossReference
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.RawCrossReferences.Count + 1,
            TypeName = typeName,
            RefText1 = parsedRef.Text1,
            RefText2 = parsedRef.Text2,
            RefSenseOrder = parsedRef.SenseOrder,
            Sense = sense,
            Type = type,
        };

        sense.RawCrossReferences.Add(crossRef);
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

#pragma warning disable IDE0060

    [LoggerMessage(LogLevel.Error,
    "Entry `{EntryId}` from corpus `{CorpusName}` contains a cross reference, which is not supported")]
    partial void LogUnsupportedCorpus(int entryId, string corpusName);

    [LoggerMessage(LogLevel.Error,
    "Third value `{ThirdValue}` in reference text `{Text}` must be an integer")]
    partial void LogNonIntegerSenseOrder(string text, string thirdValue);

    [LoggerMessage(LogLevel.Error,
    "Too many separator characters `{Separator}` in reference text `{Text}`")]
    partial void LogTooManySeparators(string text, char separator);

#pragma warning restore IDE0060

}
