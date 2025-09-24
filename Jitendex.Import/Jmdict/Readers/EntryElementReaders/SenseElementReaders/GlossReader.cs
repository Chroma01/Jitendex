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
using Jitendex.Import.Jmdict.Models;
using Jitendex.Import.Jmdict.Models.EntryElements;
using Jitendex.Import.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Import.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal class GlossReader : IJmdictReader<Sense, Gloss>
{
    private readonly ILogger<GlossReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KeywordCache _keywordCache;

    public GlossReader(ILogger<GlossReader> logger, XmlReader xmlReader, KeywordCache keywordCache) =>
        (_logger, _xmlReader, _keywordCache) =
        (@logger, @xmlReader, @keywordCache);

    public async Task ReadAsync(Sense sense)
    {
        var typeName = _xmlReader.GetAttribute("g_type");
        GlossType? type;

        if (typeName is not null)
        {
            type = _keywordCache.GetByName<GlossType>(typeName);
        }
        else
        {
            type = null;
        };

        var gloss = new Gloss
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.Glosses.Count + 1,
            Language = _xmlReader.GetAttribute("xml:lang") ?? "eng",
            TypeName = typeName,
            Type = type,
            Text = await _xmlReader.ReadElementContentAsStringAsync(),
        };

        if (gloss.Language == "eng")
        {
            sense.Glosses.Add(gloss);
        }
    }
}
