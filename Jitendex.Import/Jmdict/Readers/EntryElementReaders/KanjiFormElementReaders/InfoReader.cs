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
using Jitendex.Import.Jmdict.Models.EntryElements.KanjiFormElements;

namespace Jitendex.Import.Jmdict.Readers.EntryElementReaders.KanjiFormElementReaders;

internal class KInfoReader
{
    private readonly ILogger<KInfoReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KeywordCache _keywordCache;

    public KInfoReader(ILogger<KInfoReader> logger, XmlReader xmlReader, KeywordCache keywordCache) =>
        (_logger, _xmlReader, _keywordCache) =
        (@logger, @xmlReader, @keywordCache);

    public async Task ReadAsync(KanjiForm kanjiForm)
    {
        var description = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _keywordCache.GetByDescription<KanjiFormInfoTag>(description);

        if (kanjiForm.Infos.Any(t => t.TagName == tag.Name))
        {
            kanjiForm.Entry.IsCorrupt = true;
            Log.DuplicateTag(_logger, kanjiForm.EntryId, KanjiForm.XmlTagName, kanjiForm.Order, tag.Name, KanjiFormInfo.XmlTagName);
        }

        kanjiForm.Infos.Add(new KanjiFormInfo
        {
            EntryId = kanjiForm.EntryId,
            KanjiFormOrder = kanjiForm.Order,
            Order = kanjiForm.Infos.Count + 1,
            TagName = tag.Name,
            KanjiForm = kanjiForm,
            Tag = tag,
        });
    }
}