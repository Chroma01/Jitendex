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

internal partial class GlossReader
{
    private readonly ILogger<GlossReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KeywordCache _keywordCache;

    public GlossReader(ILogger<GlossReader> logger, XmlReader xmlReader, KeywordCache keywordCache) =>
        (_logger, _xmlReader, _keywordCache) =
        (@logger, @xmlReader, @keywordCache);

    public async Task ReadAsync(Sense sense)
    {
        var typeName = _xmlReader.GetAttribute("g_type") ?? string.Empty;
        var type = _keywordCache.GetByName<GlossType>(typeName);

        if (type.IsCorrupt)
        {
            sense.Entry.IsCorrupt = true;
        }

        var gloss = new Gloss
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.Glosses.Count + 1,
            Language = _xmlReader.GetAttribute("xml:lang") ?? "eng",
            TypeName = typeName,
            Type = type,
            Sense = sense,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };

        if (gloss.Language == "eng")
        {
            sense.Glosses.Add(gloss);
            CheckGloss(gloss);
        }
    }

    private void CheckGloss(Gloss gloss)
    {
        if (gloss.Text.Contains('\u200B'))
        {
            LogZeroWidthSpace(gloss.EntryId, gloss.SenseOrder, gloss.Order, gloss.Text);
            gloss.Sense.Entry.IsCorrupt = true;
        }
        if (gloss.Text.Contains('\u2019'))
        {
            LogRightSingleQuotationMark(gloss.EntryId, gloss.SenseOrder, gloss.Order, gloss.Text);
            gloss.Sense.Entry.IsCorrupt = true;
        }
    }

#pragma warning disable IDE0060

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} gloss #{Order} contains a right single quotation mark: `{Text}`")]
    partial void LogRightSingleQuotationMark(int entryId, int senseOrder, int order, string text);

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} gloss #{Order} contains a zero-width space: `{Text}`")]
    partial void LogZeroWidthSpace(int entryId, int senseOrder, int order, string text);

#pragma warning restore IDE0060

}
