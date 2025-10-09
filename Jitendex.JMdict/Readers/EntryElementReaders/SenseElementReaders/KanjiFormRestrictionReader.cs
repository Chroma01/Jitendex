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
using Jitendex.Import.Jmdict.Models.EntryElements;
using Jitendex.Import.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Import.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal partial class KanjiFormRestrictionReader
{
    private readonly ILogger<KanjiFormRestrictionReader> _logger;
    private readonly XmlReader _xmlReader;

    public KanjiFormRestrictionReader(ILogger<KanjiFormRestrictionReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task ReadAsync(Sense sense)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();

        var kanjiForm = sense.Entry.KanjiForms
            .Where(x => x.Text == text)
            .FirstOrDefault();

        if (kanjiForm is null)
        {
            LogInvalidKanjiFormRestriction(sense.EntryId, sense.Order, text);
            sense.Entry.IsCorrupt = true;
            return;
        }

        var restriction = new KanjiFormRestriction
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.KanjiFormRestrictions.Count + 1,
            KanjiFormOrder = kanjiForm.Order,
            KanjiForm = kanjiForm,
            Sense = sense,
        };

        sense.KanjiFormRestrictions.Add(restriction);
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{entryId}` sense #{SenseOrder} contains an invalid kanji form restriction: `{Text}`")]
    private partial void LogInvalidKanjiFormRestriction(int entryId, int senseOrder, string text);

}
