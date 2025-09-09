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
using Jitendex.Warehouse.Jmdict.Models.EntryElements.KanjiFormElements;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.KanjiFormElementReaders;

internal class KPriorityReader: IJmdictReader<KanjiForm, KanjiFormPriority>
{
    private readonly ILogger<KPriorityReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;

    public KPriorityReader(ILogger<KPriorityReader> logger, XmlReader xmlReader, DocumentTypes docTypes) =>
        (_logger, _xmlReader, _docTypes) =
        (@logger, @xmlReader, @docTypes);

    public async Task ReadAsync(KanjiForm kanjiForm)
    {
        var tagName = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _docTypes.GetKeywordByName<PriorityTag>(tagName);

        if (kanjiForm.Infos.Any(t => t.TagName == tag.Name))
        {
            kanjiForm.Entry.IsCorrupt = true;
            Log.DuplicateTag(_logger, kanjiForm.EntryId, KanjiForm.XmlTagName, kanjiForm.Order, tag.Name, KanjiFormPriority.XmlTagName);
        }

        kanjiForm.Priorities.Add(new KanjiFormPriority
        {
            EntryId = kanjiForm.EntryId,
            KanjiFormOrder = kanjiForm.Order,
            Order = kanjiForm.Priorities.Count + 1,
            TagName = tagName,
            KanjiForm = kanjiForm,
            Tag = tag,
        });
    }
}
