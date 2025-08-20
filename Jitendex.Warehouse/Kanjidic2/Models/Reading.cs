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

namespace Jitendex.Warehouse.Kanjidic2.Models;

[PrimaryKey(nameof(Character), nameof(Order))]
public class Reading
{
    public required string Character { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public required string Type { get; set; }

    [ForeignKey($"{nameof(Character)}")]
    public virtual Entry Entry { get; set; } = null!;

    internal const string XmlTagName = "reading";
}

internal static class ReadingReader
{
    public async static Task<Reading> ReadElementContentAsReadingAsync(this XmlReader reader, ReadingMeaning readingMeaning)
        => new Reading
        {
            Character = readingMeaning.Character,
            Order = readingMeaning.Readings.Count + 1,
            Type = reader.GetAttribute("r_type") ?? string.Empty,
            Text = await reader.ReadElementContentAsStringAsync(),
            Entry = readingMeaning.Entry,
        };
}
