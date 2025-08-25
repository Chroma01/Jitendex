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

internal class LanguageSourceReader
{
    private readonly XmlReader _xmlReader;
    private readonly EntityFactory _factory;
    private readonly ILogger<LanguageSourceReader> _logger;

    public LanguageSourceReader(XmlReader reader, EntityFactory factory, ILogger<LanguageSourceReader> logger)
    {
        _xmlReader = reader;
        _factory = factory;
        _logger = logger;
    }

    public async Task<LanguageSource> ReadAsync(Sense sense)
    {
        var typeName = _xmlReader.GetAttribute("ls_type") ?? "full";
        var languageCode = _xmlReader.GetAttribute("xml:lang") ?? "eng";
        var wasei = _xmlReader.GetAttribute("ls_wasei");
        if (wasei is not null && wasei != "y")
        {
            // TODO: Log and warn
        }
        var text = _xmlReader.IsEmptyElement ? null : await _xmlReader.ReadElementContentAsStringAsync();
        return new LanguageSource
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.LanguageSources.Count + 1,
            Text = text,
            LanguageCode = languageCode,
            TypeName = typeName,
            IsWasei = wasei == "y",
            Language = _factory.GetKeywordByName<Language>(languageCode),
            Type = _factory.GetKeywordByName<LanguageSourceType>(typeName),
        };
    }
}
