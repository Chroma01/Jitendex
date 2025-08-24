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

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements.ReadingElements;

internal static class PriorityReader
{
    public async static Task<Priority> ReadPriorityAsync(this XmlReader reader, Reading reading, EntityFactory factory)
    {
        var tagName = await reader.ReadElementContentAsStringAsync();
        var tag = factory.GetKeywordByName<PriorityTag>(tagName);
        return new Priority
        {
            EntryId = reading.EntryId,
            ReadingOrder = reading.Order,
            TagName = tagName,
            Reading = reading,
            Tag = tag,
        };
    }
}
