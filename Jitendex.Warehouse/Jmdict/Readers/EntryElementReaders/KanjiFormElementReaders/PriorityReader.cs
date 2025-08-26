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

internal class KPriorityReader: IJmdictReader<KanjiForm, KPriority>
{
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;
    private readonly ILogger<KPriorityReader> _logger;

    public KPriorityReader(XmlReader xmlReader, DocumentTypes docTypes, ILogger<KPriorityReader> logger) =>
        (_xmlReader, _docTypes, _logger) =
        (xmlReader, docTypes, logger);

    public async Task<KPriority> ReadAsync(KanjiForm kanjiForm)
    {
        var tagName = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _docTypes.GetKeywordByName<PriorityTag>(tagName);
        return new KPriority
        {
            EntryId = kanjiForm.EntryId,
            KanjiFormOrder = kanjiForm.Order,
            TagName = tagName,
            KanjiForm = kanjiForm,
            Tag = tag,
        };
    }
}
