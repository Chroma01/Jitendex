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
using Jitendex.Warehouse.Jmdict.Models.EntryElements.ReadingElements;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.ReadingElementReaders;

internal partial class RestrictionReader : IJmdictReader<Reading, Restriction>
{
    private readonly ILogger<RestrictionReader> _logger;
    private readonly XmlReader _xmlReader;

    public RestrictionReader(ILogger<RestrictionReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task ReadAsync(Reading reading)
    {
        var kanjiFormText = await _xmlReader.ReadElementContentAsStringAsync();

        var matchingKanjiForms = reading.Entry.KanjiForms
            .Where(k => k.Text == kanjiFormText);

        if (matchingKanjiForms.Count() != 1)
        {
            LogInvalidRestriction(reading.EntryId, reading.Order, kanjiFormText);
            reading.Entry.IsCorrupt = true;
            return;
        }

        var kanjiForm = matchingKanjiForms.First();
        if (kanjiForm.IsHidden())
        {
            LogHiddenRestriction(reading.EntryId, reading.Order, kanjiFormText);
            reading.Entry.IsCorrupt = true;
            return;   
        }

        var restriction = new Restriction
        {
            EntryId = reading.EntryId,
            ReadingOrder = reading.Order,
            Order = reading.Restrictions.Count + 1,
            KanjiFormOrder = kanjiForm.Order,
            Reading = reading,
            KanjiForm = kanjiForm,
        };

        reading.Restrictions.Add(restriction);
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` reading #{Order} contains an invalid kanji restriction: `{KanjiFormText}`")]
    private partial void LogInvalidRestriction(int entryId, int order, string kanjiFormText);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` reading #{Order} contains a restriction to a hidden kanji form: `{KanjiFormText}`")]
    private partial void LogHiddenRestriction(int entryId, int order, string kanjiFormText);

}
