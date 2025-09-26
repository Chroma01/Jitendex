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
using Jitendex.Import.Jmdict.Models;
using Jitendex.Import.Jmdict.Models.EntryElements;
using Jitendex.Import.Jmdict.Models.EntryElements.SenseElements;
using Jitendex.Import.Jmdict.Readers.DocumentTypes;

namespace Jitendex.Import.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal partial class LanguageSourceReader
{
    private readonly ILogger<LanguageSourceReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KeywordCache _keywordCache;

    public LanguageSourceReader(ILogger<LanguageSourceReader> logger, XmlReader xmlReader, KeywordCache keywordCache) =>
        (_logger, _xmlReader, _keywordCache) =
        (@logger, @xmlReader, @keywordCache);

    public async Task ReadAsync(Sense sense)
    {
        var typeName = _xmlReader.GetAttribute("ls_type") ?? "full";
        var type = _keywordCache.GetByName<LanguageSourceType>(typeName);

        var languageCode = _xmlReader.GetAttribute("xml:lang") ?? "eng";
        var language = _keywordCache.GetByName<Language>(languageCode);

        if (type.IsCorrupt || language.IsCorrupt)
        {
            sense.Entry.IsCorrupt = true;
        }

        var wasei = _xmlReader.GetAttribute("ls_wasei");
        if (wasei is not null && wasei != "y")
        {
            LogInvalidWaseiValue(sense.EntryId, sense.Order, wasei);
            sense.Entry.IsCorrupt = true;
        }

        string? text;
        if (_xmlReader.IsEmptyElement)
        {
            text = null;
        }
        else
        {
            text = await _xmlReader.ReadElementContentAsStringAsync();
        }

        var languageSource = new LanguageSource
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.LanguageSources.Count + 1,
            Text = text,
            LanguageCode = languageCode,
            TypeName = typeName,
            IsWasei = wasei == "y",
            Sense = sense,
            Language = language,
            Type = type,
        };

        sense.LanguageSources.Add(languageSource);
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` sense #{SenseOrder} has a language source WASEI attribute with an invalid value: `{Value}`")]
    private partial void LogInvalidWaseiValue(int entryId, int senseOrder, string value);

}
