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

using Microsoft.Extensions.Logging;
using Jitendex.KanjiVG.Models;
using Jitendex.KanjiVG.Readers.Metadata;

namespace Jitendex.KanjiVG.Readers;

internal partial class KanjiVGReader
{
    private readonly ILogger<KanjiVGReader> _logger;
    private readonly EntriesReader _entriesReader;
    private readonly ComponentGroupStyleCache _componentGroupStyleCache;

    public KanjiVGReader(
        ILogger<KanjiVGReader> logger,
        EntriesReader entriesReader,
        ComponentGroupStyleCache componentGroupStyleCache) =>
        (_logger, _entriesReader, _componentGroupStyleCache) =
        (@logger, @entriesReader, @componentGroupStyleCache);

    public async Task<KanjiVGDocument> ReadAsync()
    {
        var entries = await _entriesReader.ReadAsync();

        var kanjiVG = new KanjiVGDocument
        {
            Entries = entries,
            ComponentGroupStyles = [.. _componentGroupStyleCache.Values()],
        };

        return kanjiVG;
    }
}
