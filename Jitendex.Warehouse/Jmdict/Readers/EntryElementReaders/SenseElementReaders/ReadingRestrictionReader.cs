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
using Jitendex.Warehouse.Jmdict.Models.EntryElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal partial class ReadingRestrictionReader : IJmdictReader<Sense, ReadingRestriction>
{
    private readonly ILogger<DialectReader> _logger;
    private readonly XmlReader _xmlReader;

    public ReadingRestrictionReader(ILogger<DialectReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task ReadAsync(Sense sense)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();
        var restriction = new ReadingRestriction
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.ReadingRestrictions.Count + 1,
            ReadingOrder = -1,
            Sense = sense,
        };
        var reading = sense.Entry.Readings
            .Where(x => x.Text == text)
            .FirstOrDefault();
        if (reading is not null)
        {
            restriction.ReadingOrder = reading.Order;
            restriction.Reading = reading;
            sense.ReadingRestrictions.Add(restriction);
        }
        else
        {
            LogInvalidReadingRestriction(sense.EntryId, sense.Order, text);
            sense.Entry.IsCorrupt = true;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} contains an invalid reading restriction: `{Text}`")]
    private partial void LogInvalidReadingRestriction(int entryId, int senseOrder, string text);

}
