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

internal class InfoReader
{
    private readonly XmlReader _xmlReader;
    private readonly EntityFactory _factory;
    private readonly ILogger<InfoReader> _logger;

    public InfoReader(XmlReader reader, EntityFactory factory, ILogger<InfoReader> logger)
    {
        _xmlReader = reader;
        _factory = factory;
        _logger = logger;
    }

    public async Task<Info> ReadAsync(KanjiForm kanjiForm)
    {
        var description = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _factory.GetKeywordByDescription<KanjiFormInfoTag>(description);
        return new Info
        {
            EntryId = kanjiForm.EntryId,
            KanjiFormOrder = kanjiForm.Order,
            TagName = tag.Name,
            KanjiForm = kanjiForm,
            Tag = tag,
        };
    }
}