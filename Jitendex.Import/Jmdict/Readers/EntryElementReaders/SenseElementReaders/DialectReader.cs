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
using Jitendex.Import.Jmdict.Readers.DocumentTypes;

namespace Jitendex.Import.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal class DialectReader
{
    private readonly ILogger<DialectReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KeywordCache _keywordCache;

    public DialectReader(ILogger<DialectReader> logger, XmlReader xmlReader, KeywordCache keywordCache) =>
        (_logger, _xmlReader, _keywordCache) =
        (@logger, @xmlReader, @keywordCache);

    public async Task ReadAsync(Sense sense)
    {
        var description = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _keywordCache.GetByDescription<DialectTag>(description);

        if (sense.Dialects.Any(t => t.TagName == tag.Name))
        {
            sense.Entry.IsCorrupt = true;
            Log.DuplicateTag(_logger, sense.EntryId, Sense.XmlTagName, sense.Order, tag.Name, Dialect.XmlTagName);
        }

        sense.Dialects.Add(new Dialect
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.Dialects.Count + 1,
            TagName = tag.Name,
            Sense = sense,
            Tag = tag,
        });
    }
}
