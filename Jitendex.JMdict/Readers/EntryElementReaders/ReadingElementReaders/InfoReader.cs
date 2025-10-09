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
using Jitendex.JMdict.Models.EntryElements.ReadingElements;
using Jitendex.JMdict.Readers.DocumentTypes;

namespace Jitendex.JMdict.Readers.EntryElementReaders.ReadingElementReaders;

internal class RInfoReader
{
    private readonly ILogger<RInfoReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KeywordCache _keywordCache;

    public RInfoReader(ILogger<RInfoReader> logger, XmlReader xmlReader, KeywordCache keywordCache) =>
        (_logger, _xmlReader, _keywordCache) =
        (@logger, @xmlReader, @keywordCache);

    public async Task ReadAsync(Reading reading)
    {
        var description = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _keywordCache.GetByDescription<ReadingInfoTag>(description);

        if (tag.IsCorrupt)
        {
            reading.Entry.IsCorrupt = true;
        }

        if (reading.Infos.Any(t => t.TagName == tag.Name))
        {
            reading.Entry.IsCorrupt = true;
            Log.DuplicateTag(_logger, reading.EntryId, Reading.XmlTagName, reading.Order, tag.Name, ReadingInfo.XmlTagName);
        }

        var info = new ReadingInfo
        {
            EntryId = reading.EntryId,
            ReadingOrder = reading.Order,
            Order = reading.Infos.Count + 1,
            TagName = tag.Name,
            Reading = reading,
            Tag = tag,
        };

        reading.Infos.Add(info);
    }
}
