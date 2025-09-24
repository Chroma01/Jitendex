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
using Jitendex.Import.Jmdict.Models;

namespace Jitendex.Import.Jmdict.Readers;

internal partial class ExampleCache
{
    private readonly ILogger<ExampleCache> _logger;
    private readonly KeywordCache _keywordCache;

    public ExampleCache(ILogger<ExampleCache> logger, KeywordCache keywordCache) =>
        (_logger, _keywordCache) =
        (@logger, @keywordCache);

    private readonly Dictionary<(string, int), ExampleSource> _cache = [];

    public ExampleSource GetExampleSource(string typeName, int originKey)
    {
        var cacheKey = (typeName, originKey);

        if (_cache.TryGetValue(cacheKey, out ExampleSource? exampleSource))
        {
            return exampleSource;
        }

        var newExampleSource = new ExampleSource
        {
            TypeName = typeName,
            OriginKey = originKey,
            Text = string.Empty,
            Translation = string.Empty,
            ExampleSourceType = _keywordCache.GetByName<ExampleSourceType>(typeName),
        };

        _cache.Add(cacheKey, newExampleSource);
        return newExampleSource;
    }
}
