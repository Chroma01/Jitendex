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
using Jitendex.KanjiVG.Readers.Lookups;

namespace Jitendex.KanjiVG.Readers;

internal partial class KanjiVGReader
{
    private readonly ILogger<KanjiVGReader> _logger;
    private readonly EntriesReader _entriesReader;
    private readonly VariantTypeCache _variantTypeCache;
    private readonly ComponentGroupStyleCache _componentGroupStyleCache;
    private readonly StrokeNumberGroupStyleCache _strokeNumberGroupStyleCache;
    private readonly ComponentPositionCache _positionCache;
    private readonly ComponentRadicalCache _radicalCache;
    private readonly StrokeTypeCache _strokeTypeCache;

    public KanjiVGReader(
        ILogger<KanjiVGReader> logger,
        EntriesReader entriesReader,
        VariantTypeCache variantTypeCache,
        ComponentGroupStyleCache componentGroupStyleCache,
        StrokeNumberGroupStyleCache strokeNumberGroupStyleCache,
        ComponentPositionCache positionCache,
        ComponentRadicalCache radicalCache,
        StrokeTypeCache strokeTypeCache) =>
        (_logger, _entriesReader, _variantTypeCache, _componentGroupStyleCache, _strokeNumberGroupStyleCache, _positionCache, _radicalCache, _strokeTypeCache) =
        (@logger, @entriesReader, @variantTypeCache, @componentGroupStyleCache, @strokeNumberGroupStyleCache, @positionCache, @radicalCache, @strokeTypeCache);

    public async Task<KanjiVGDocument> ReadAsync()
    {
        var entries = await _entriesReader.ReadAsync();

        var kanjivg = new KanjiVGDocument
        {
            Entries = entries,
            VariantTypes = [.. _variantTypeCache.Values],
            ComponentGroupStyles = [.. _componentGroupStyleCache.Values],
            StrokeNumberGroupStyles = [.. _strokeNumberGroupStyleCache.Values],
            ComponentPositions = [.. _positionCache.Values],
            ComponentRadicals = [.. _radicalCache.Values],
            StrokeTypes = [.. _strokeTypeCache.Values],
        };

        return kanjivg;
    }
}
