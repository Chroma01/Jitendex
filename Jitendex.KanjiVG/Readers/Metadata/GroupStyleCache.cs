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

using Jitendex.KanjiVG.Models;

namespace Jitendex.KanjiVG.Readers.Metadata;

internal abstract class GroupStyleCache<T> where T: IGroupStyle
{
    private readonly Dictionary<string, T> _cache = new();
    public IEnumerable<T> Values() => _cache.Values;

    public T GetGroupStyle(Entry entry, string text)
    {
        if (!IsKnownStyle(text))
        {
            LogUnknownStyle(entry.FileName(), text);
        }

        if (_cache.TryGetValue(text, out T? componentGroupStyle))
        {
            return componentGroupStyle;
        }

        int id = _cache.Count + 1;
        componentGroupStyle = NewGroup(id, text);

        _cache.Add(text, componentGroupStyle);
        return componentGroupStyle;
    }

    protected abstract T NewGroup(int id, string text);
    protected abstract bool IsKnownStyle(string style);
    protected abstract void LogUnknownStyle(string file, string style);
}
