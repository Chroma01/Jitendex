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

internal static class LanguageSourceReader
{
    public async static Task<LanguageSource> ReadLanguageSourceAsync(this XmlReader reader, Sense sense, EntityFactory factory)
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
            Language = factory.GetKeywordByName<Language>(languageCode),
            Type = factory.GetKeywordByName<LanguageSourceType>(typeName),
        };
    }
}
