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

internal class KInfoReader : IJmdictReader<KanjiForm, KanjiFormInfo>
{
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;
    private readonly ILogger<KInfoReader> _logger;

    public KInfoReader(XmlReader xmlReader, DocumentTypes docTypes, ILogger<KInfoReader> logger) =>
        (_xmlReader, _docTypes, _logger) =
        (@xmlReader, @docTypes, @logger);

    public async Task<KanjiFormInfo> ReadAsync(KanjiForm kanjiForm)
    {
        var description = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _docTypes.GetKeywordByDescription<KanjiFormInfoTag>(description);
        return new KanjiFormInfo
        {
            EntryId = kanjiForm.EntryId,
            KanjiFormOrder = kanjiForm.Order,
            TagName = tag.Name,
            KanjiForm = kanjiForm,
            Tag = tag,
        };
    }
}