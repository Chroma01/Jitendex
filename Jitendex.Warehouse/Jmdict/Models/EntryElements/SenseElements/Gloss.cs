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

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(Order))]
public class Gloss
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required int Order { get; set; }

    public required string Text { get; set; }
    public string? TypeName { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TypeName))]
    public virtual GlossType? Type { get; set; }

    [NotMapped]
    internal string? Language { get; set; }

    internal const string XmlTagName = "gloss";
}

internal static class GlossReader
{
    public async static Task<Gloss> ReadGlossAsync(this XmlReader reader, Sense sense, KeywordFactory factory)
    {
        var typeName = reader.GetAttribute("g_type");
        var type = typeName is not null ?
            factory.GetByName<GlossType>(typeName) : null;
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
