/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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
using Jitendex.JMdict.Import.Models;
using Jitendex.JMdict.Import.Models.EntryElements;
using Jitendex.JMdict.Import.Models.EntryElements.SenseElements;

namespace Jitendex.JMdict.Import.Parsing.EntryElementReaders.SenseElementReaders;

internal partial class LanguageSourceReader : BaseReader<LanguageSourceReader>
{
    public LanguageSourceReader(ILogger<LanguageSourceReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    public async Task ReadAsync(Document document, SenseElement sense)
    {
        var typeName = _xmlReader.GetAttribute("ls_type") ?? "full";
        if (!document.LanguageSourceTypes.ContainsKey(typeName))
        {
            var tag = new LanguageSourceTypeElement(typeName, document.Header.Date);
            document.LanguageSourceTypes.Add(typeName, tag);
        }

        var languageCode = _xmlReader.GetAttribute("xml:lang") ?? "eng";
        if (!document.Languages.ContainsKey(languageCode))
        {
            var tag = new LanguageElement(languageCode, document.Header.Date);
            document.Languages.Add(languageCode, tag);
        }

        var wasei = _xmlReader.GetAttribute("ls_wasei");
        if (wasei is not null && wasei != "y")
        {
            LogInvalidWaseiValue(sense.EntryId, sense.Order, wasei);
        }

        var text = _xmlReader.IsEmptyElement
            ? null
            : await _xmlReader.ReadElementContentAsStringAsync();

        var languageSource = new LanguageSourceElement
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = document.NextOrder(sense.Key(), nameof(LanguageSourceElement)),
            Text = text,
            LanguageCode = languageCode,
            TypeName = typeName,
            IsWasei = wasei == "y",
        };

        document.LanguageSources.Add(languageSource.Key(), languageSource);
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` sense #{SenseOrder} has a language source WASEI attribute with an invalid value: `{Value}`")]
    partial void LogInvalidWaseiValue(int entryId, int senseOrder, string value);
}
