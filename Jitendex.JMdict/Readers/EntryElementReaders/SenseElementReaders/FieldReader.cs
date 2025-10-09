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

internal class FieldReader
{
    private readonly ILogger<FieldReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KeywordCache _keywordCache;

    public FieldReader(ILogger<FieldReader> logger, XmlReader xmlReader, KeywordCache keywordCache) =>
        (_logger, _xmlReader, _keywordCache) =
        (@logger, @xmlReader, @keywordCache);

    public async Task ReadAsync(Sense sense)
    {
        var description = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _keywordCache.GetByDescription<FieldTag>(description);

        if (tag.IsCorrupt)
        {
            sense.Entry.IsCorrupt = true;
        }

        if (sense.Fields.Any(t => t.TagName == tag.Name))
        {
            sense.Entry.IsCorrupt = true;
            Log.DuplicateTag(_logger, sense.EntryId, Sense.XmlTagName, sense.Order, tag.Name, Field.XmlTagName);
        }

        var field = new Field
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.Fields.Count + 1,
            TagName = tag.Name,
            Sense = sense,
            Tag = tag,
        };

        sense.Fields.Add(field);
    }
}
