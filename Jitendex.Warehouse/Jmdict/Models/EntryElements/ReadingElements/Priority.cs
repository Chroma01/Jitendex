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

using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements.ReadingElements;

[Table($"{nameof(Reading)}{nameof(Priority)}")]
[PrimaryKey(nameof(EntryId), nameof(ReadingOrder), nameof(TagName))]
public class Priority
{
    public required int EntryId { get; set; }
    public required int ReadingOrder { get; set; }
    public required string TagName { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(ReadingOrder)}")]
    public virtual Reading Reading { get; set; } = null!;

    [ForeignKey(nameof(TagName))]
    public virtual PriorityTag Tag { get; set; } = null!;

    internal const string XmlTagName = "re_pri";
}

internal static class PriorityReader
{
    public async static Task<Priority> ReadPriorityAsync(this XmlReader reader, Reading reading, KeywordFactory factory)
    {
        var tagName = await reader.ReadElementContentAsStringAsync();
        var tag = factory.GetByName<PriorityTag>(tagName);
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
