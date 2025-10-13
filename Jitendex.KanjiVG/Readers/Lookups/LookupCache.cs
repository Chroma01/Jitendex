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

using System.Collections.Immutable;
using Jitendex.KanjiVG.Models;
using Microsoft.Extensions.Logging;

namespace Jitendex.KanjiVG.Readers.Lookups;

internal abstract class LookupCache<T> where T : ILookup
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, int> _textToId;
    private readonly Dictionary<string, T> _cache;
    public IEnumerable<T> Values { get => _cache.Values; }

    public LookupCache(ILogger logger)
    {
        _logger = logger;
        _textToId = KnownLookups()
            .Select(static (text, index) =>
                new KeyValuePair<string, int>(text, index + 1))
            .ToDictionary();
        _cache = new(_textToId.Count);
    }

    public T Get(string text)
    {
        if (_cache.TryGetValue(text, out T? lookup))
        {
            return lookup;
        }

        if (!_textToId.TryGetValue(text, out int id))
        {
            LogUnknownLookup(text);
            id = _textToId.Count + 1;
            _textToId[text] = id;
        }

        lookup = NewLookup(id, text);

        _cache.Add(text, lookup);
        return lookup;
    }

    private void LogUnknownLookup(string text)
    {
        _logger.LogWarning
        (
            "Type `{TypeName}` with value `{Text}` has never been seen before",
            typeof(T).Name,
            text
        );
    }

    protected abstract T NewLookup(int id, string text);
    protected abstract ImmutableArray<string> KnownLookups();
}
