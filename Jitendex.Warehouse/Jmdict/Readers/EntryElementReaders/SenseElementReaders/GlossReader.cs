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
using Jitendex.Warehouse.Jmdict.Models;
using Jitendex.Warehouse.Jmdict.Models.EntryElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal static class GlossReader
{
    public async static Task<Gloss> ReadGlossAsync(this XmlReader reader, Sense sense, EntityFactory factory)
    {
        var typeName = reader.GetAttribute("g_type");
        var type = typeName is not null ?
            factory.GetKeywordByName<GlossType>(typeName) : null;
        return new Gloss
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.Glosses.Count + 1,
            Language = reader.GetAttribute("xml:lang") ?? "eng",
            TypeName = typeName,
            Type = type,
            Text = await reader.ReadElementContentAsStringAsync(),
        };
    }
}
