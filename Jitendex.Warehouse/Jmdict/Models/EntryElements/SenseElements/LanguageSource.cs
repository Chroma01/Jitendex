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
public class LanguageSource
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required int Order { get; set; }

    public required string? Text { get; set; }
    public required string LanguageCode { get; set; }
    public required string TypeName { get; set; }
    public required bool IsWasei { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(LanguageCode))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey(nameof(TypeName))]
    public virtual LanguageSourceType Type { get; set; } = null!;

    internal const string XmlTagName = "lsource";
}

internal static class LanguageSourceReader
{
    public async static Task<LanguageSource> ReadLanguageSourceAsync(this XmlReader reader, Sense sense, KeywordFactory factory)
    {
        var typeName = reader.GetAttribute("ls_type") ?? "full";
        var languageCode = reader.GetAttribute("xml:lang") ?? "eng";
        var wasei = reader.GetAttribute("ls_wasei");
        if (wasei is not null && wasei != "y")
        {
            // TODO: Log and warn
        }
        var text = reader.IsEmptyElement ? null : await reader.ReadElementContentAsStringAsync();
        return new LanguageSource
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.LanguageSources.Count + 1,
            Text = text,
            LanguageCode = languageCode,
            TypeName = typeName,
            IsWasei = wasei == "y",
            Language = factory.GetByName<Language>(languageCode),
            Type = factory.GetByName<LanguageSourceType>(typeName),
        };
    }
}
