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

internal partial class StrokeNumberGroupStyleCache(ILogger<StrokeNumberGroupStyleCache> logger) : GroupStyleCache<StrokeNumberGroupStyle>
{
    protected override StrokeNumberGroupStyle NewGroup(int id, string text) => new StrokeNumberGroupStyle
    {
        Id = id,
        Text = text,
    };

    protected override bool IsKnownStyle(string style) => style switch
    {
        "font-size:8;fill:#808080" => true,
        _ => false
    };

    [LoggerMessage(LogLevel.Warning,
    "File `{File}` contains a stroke number group with an unknown style attribute: `{Style}`")]
    protected override partial void LogUnknownStyle(string file, string style);
}
