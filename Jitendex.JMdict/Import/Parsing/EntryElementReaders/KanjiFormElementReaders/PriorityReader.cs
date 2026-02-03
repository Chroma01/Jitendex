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

namespace Jitendex.JMdict.Import.Parsing.EntryElementReaders.KanjiFormElementReaders;

internal class KPriorityReader : BaseReader<KPriorityReader>
{
    public KPriorityReader(ILogger<KPriorityReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    public async Task ReadAsync(Document document, KanjiFormElement kanjiForm)
    {
        var tagName = await _xmlReader.ReadElementContentAsStringAsync();

        if (!document.PriorityTags.ContainsKey(tagName))
        {
            var tag = new PriorityTagElement(tagName, document.Header.Date);
            document.PriorityTags.Add(tagName, tag);
        }

        var priority = new KanjiFormPriorityElement
        (
            EntryId: kanjiForm.EntryId,
            KanjiFormOrder: kanjiForm.Order,
            Order: document.KanjiFormPriorities.NextOrder(kanjiForm.Key()),
            TagName: tagName
        );

        document.KanjiFormPriorities.Add(priority.Key(), priority);
    }
}
