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
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal class FieldReader : IJmdictReader<Sense, Field>
{
    private readonly ILogger<FieldReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;

    public FieldReader(ILogger<FieldReader> logger, XmlReader xmlReader, DocumentTypes docTypes) =>
        (_logger, _xmlReader, _docTypes) =
        (@logger, @xmlReader, @docTypes);

    public async Task<Field> ReadAsync(Sense sense)
    {
        var description = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _docTypes.GetKeywordByDescription<FieldTag>(description);
        return new Field
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            TagName = tag.Name,
            Sense = sense,
            Tag = tag,
        };
    }
}
