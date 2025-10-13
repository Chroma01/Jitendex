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

namespace Jitendex.KanjiVG.Readers.Metadata;

internal partial class ComponentGroupStyleCache(ILogger<ComponentGroupStyleCache> logger)
{
    private readonly Dictionary<string, ComponentGroupStyle> _cache = new();
    public IEnumerable<ComponentGroupStyle> Values() => _cache.Values;

    public ComponentGroupStyle GetComponentGroupStyle(Entry entry, string style)
    {
        if (!IsKnownStyle(style))
        {
            LogUnknownStyle(entry.FileName(), style);
        }

        if (_cache.TryGetValue(style, out ComponentGroupStyle? componentGroupStyle))
        {
            return componentGroupStyle;
        }

        componentGroupStyle = new ComponentGroupStyle
        {
            Id = _cache.Count + 1,
            Text = style,
        };

        _cache.Add(style, componentGroupStyle);
        return componentGroupStyle;
    }

    private static bool IsKnownStyle(string style) => style switch
    {
        "fill:none;stroke:#000000;stroke-width:3;stroke-linecap:round;stroke-linejoin:round;" => true,
        "fill:#000000;stroke:#000000;stroke-width:3;stroke-linecap:round;stroke-linejoin:round;" => true,
        _ => false
    };

    [LoggerMessage(LogLevel.Warning,
    "File `{File}` contains a component group with an unknown style attribute: `{Style}`")]
    private partial void LogUnknownStyle(string file, string style);
}
