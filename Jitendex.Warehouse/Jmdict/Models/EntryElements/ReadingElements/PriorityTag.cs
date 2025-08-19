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

[Table($"{nameof(Reading)}{nameof(PriorityTag)}")]
[PrimaryKey(nameof(EntryId), nameof(ReadingOrder), nameof(TagId))]
public class PriorityTag
{
    public required int EntryId { get; set; }
    public required int ReadingOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(ReadingOrder)}")]
    public virtual Reading Reading { get; set; } = null!;

    #region Static XML Factory

    internal const string XmlTagName = "re_pri";

    internal async static Task<PriorityTag> FromXmlAsync(XmlReader reader, Reading reading)
        => new PriorityTag
        {
            EntryId = reading.EntryId,
            ReadingOrder = reading.Order,
            TagId = await reader.ReadAndGetTextValueAsync(),
            Reading = reading,
        };

    #endregion
}
