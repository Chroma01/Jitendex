/*
Copyright (c) 2025 Stephen Kraus
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
using Jitendex.JMdict.Models;
using Jitendex.JMdict.Models.EntryElements;
using Jitendex.JMdict.Models.EntryElements.KanjiFormElements;
using Jitendex.JMdict.Readers.DocumentTypes;

namespace Jitendex.JMdict.Readers.EntryElementReaders.KanjiFormElementReaders;

internal class KPriorityReader
{
    private readonly ILogger<KPriorityReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KeywordCache _keywordCache;

    public KPriorityReader(ILogger<KPriorityReader> logger, XmlReader xmlReader, KeywordCache @keywordCache) =>
        (_logger, _xmlReader, _keywordCache) =
        (@logger, @xmlReader, @keywordCache);

    public async Task ReadAsync(KanjiForm kanjiForm)
    {
        var tagName = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _keywordCache.GetByName<PriorityTag>(tagName);

        if (tag.IsCorrupt)
        {
            kanjiForm.Entry.IsCorrupt = true;
        }

        if (kanjiForm.Infos.Any(t => t.TagName == tag.Name))
        {
            kanjiForm.Entry.IsCorrupt = true;
            Log.DuplicateTag(_logger, kanjiForm.EntryId, KanjiForm.XmlTagName, kanjiForm.Order, tag.Name, KanjiFormPriority.XmlTagName);
        }

        var priority = new KanjiFormPriority
        {
            EntryId = kanjiForm.EntryId,
            KanjiFormOrder = kanjiForm.Order,
            Order = kanjiForm.Priorities.Count + 1,
            TagName = tagName,
            KanjiForm = kanjiForm,
            Tag = tag,
        };

        kanjiForm.Priorities.Add(priority);
    }
}
