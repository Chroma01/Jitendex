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

internal class LanguageSourceReader
{
    private readonly XmlReader Reader;
    private readonly EntityFactory Factory;

    public LanguageSourceReader(XmlReader reader, EntityFactory factory)
    {
        Reader = reader;
        Factory = factory;
    }

    public async Task<LanguageSource> ReadAsync(Sense sense)
    {
        var typeName = Reader.GetAttribute("ls_type") ?? "full";
        var languageCode = Reader.GetAttribute("xml:lang") ?? "eng";
        var wasei = Reader.GetAttribute("ls_wasei");
        if (wasei is not null && wasei != "y")
        {
            // TODO: Log and warn
        }
        var text = Reader.IsEmptyElement ? null : await Reader.ReadElementContentAsStringAsync();
        return new LanguageSource
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.LanguageSources.Count + 1,
            Text = text,
            LanguageCode = languageCode,
            TypeName = typeName,
            IsWasei = wasei == "y",
            Language = Factory.GetKeywordByName<Language>(languageCode),
            Type = Factory.GetKeywordByName<LanguageSourceType>(typeName),
        };
    }
}
