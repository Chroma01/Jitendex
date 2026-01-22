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

internal class PartOfSpeechReader : BaseReader<PartOfSpeechReader>
{
    public PartOfSpeechReader(ILogger<PartOfSpeechReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    public async Task ReadAsync(Document document,Sense sense)
    {
        var description = await _xmlReader.ReadElementContentAsStringAsync();

        if (!document.KeywordDescriptionToName.TryGetValue(description, out var tagName))
        {
            tagName = description;
            // TODO: Log missing tag name
        }

        if (!document.PartOfSpeechTags.ContainsKey(tagName))
        {
            var tag = new PartOfSpeechTag
            {
                Name = tagName,
                CreatedDate = document.FileHeader.Date,
            };
            document.PartOfSpeechTags.Add(tagName, tag);
        }

        var partOfSpeech = new PartOfSpeech
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = document.NextOrder(sense.Key(), nameof(PartOfSpeech)),
            TagName = tagName,
        };

        document.PartsOfSpeech.Add(partOfSpeech.Key(), partOfSpeech);
    }
}
