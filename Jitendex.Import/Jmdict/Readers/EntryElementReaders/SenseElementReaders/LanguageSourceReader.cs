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

namespace Jitendex.Import.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal partial class LanguageSourceReader : IJmdictReader<Sense, LanguageSource>
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
        var languageCode = _xmlReader.GetAttribute("xml:lang") ?? "eng";

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

        sense.LanguageSources.Add(new LanguageSource
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.LanguageSources.Count + 1,
            Text = text,
            LanguageCode = languageCode,
            TypeName = typeName,
            IsWasei = wasei == "y",
            Language = _keywordCache.GetByName<Language>(languageCode),
            Type = _keywordCache.GetByName<LanguageSourceType>(typeName),
        });
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` sense #{SenseOrder} has a language source WASEI attribute with an invalid value: `{Value}`")]
    private partial void LogInvalidWaseiValue(int entryId, int senseOrder, string value);

}
