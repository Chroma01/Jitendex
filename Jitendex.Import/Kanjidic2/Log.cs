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

namespace Jitendex.Import.Kanjidic2;

internal static partial class Log
{
    [LoggerMessage(LogLevel.Warning,
    "`{Character}`: Unexpected XML element node <{TagName}> found in element <{ParentTagName}>")]
    public static partial void UnexpectedChildElement(ILogger logger, string character, string tagName, string parentTagName);

    [LoggerMessage(LogLevel.Warning,
    "`{Character}`: Unexpected XML text node found in element <{TagName}>: `{Text}`")]
    public static partial void UnexpectedTextNode(ILogger logger, string character, string tagName, string text);
}
