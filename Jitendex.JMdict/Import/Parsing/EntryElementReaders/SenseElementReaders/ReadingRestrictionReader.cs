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

internal partial class ReadingRestrictionReader : BaseReader<ReadingRestrictionReader>
{
    public ReadingRestrictionReader(ILogger<ReadingRestrictionReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    public async Task ReadAsync(Document document, SenseElement sense)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();

        var restriction = new ReadingRestrictionElement
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = document.ReadingRestrictions.NextOrder(sense.Key()),
            ReadingText = text,
        };

        document.ReadingRestrictions.Add(restriction.Key(), restriction);
    }
}
