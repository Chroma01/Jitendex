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
using Jitendex.Import.Jmdict.Models.EntryElements.ReadingElements;

namespace Jitendex.Import.Jmdict.Readers.EntryElementReaders.ReadingElementReaders;

internal class RInfoReader : IJmdictReader<Reading, ReadingInfo>
{
    private readonly ILogger<RInfoReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;

    public RInfoReader(ILogger<RInfoReader> logger, XmlReader xmlReader, DocumentTypes docTypes) =>
        (_logger, _xmlReader, _docTypes) =
        (@logger, @xmlReader, @docTypes);

    public async Task ReadAsync(Reading reading)
    {
        var description = await _xmlReader.ReadElementContentAsStringAsync();
        var tag = _docTypes.GetKeywordByDescription<ReadingInfoTag>(description);

        if (reading.Infos.Any(t => t.TagName == tag.Name))
        {
            reading.Entry.IsCorrupt = true;
            Log.DuplicateTag(_logger, reading.EntryId, Reading.XmlTagName, reading.Order, tag.Name, ReadingInfo.XmlTagName);
        }

        reading.Infos.Add(new ReadingInfo
        {
            EntryId = reading.EntryId,
            ReadingOrder = reading.Order,
            Order = reading.Infos.Count + 1,
            TagName = tag.Name,
            Reading = reading,
            Tag = tag,
        });
    }
}
