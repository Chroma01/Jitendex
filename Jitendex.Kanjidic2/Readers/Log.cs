/*
Copyright (c) 2025 Stephen Kraus
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

using System.Text;
using Microsoft.Extensions.Logging;

namespace Jitendex.Kanjidic2.Readers;

internal static partial class Log
{
    public static void UnexpectedChildElement(ILogger logger, Rune character, string tagName, string parentTagName)
        => LogUnexpectedChildElement(logger, character, tagName, parentTagName);

    public static void UnexpectedTextNode(ILogger logger, Rune character, string tagName, string text)
        => LogUnexpectedTextNode(logger, character, tagName, text);

    public static void Duplicate(ILogger logger, Rune character, string parentTagName, string text, string tagName)
        => LogDuplicate(logger, character, parentTagName, text, tagName);


    [LoggerMessage(LogLevel.Warning,
    "`{Character}`: Unexpected XML element node <{TagName}> found in element <{ParentTagName}>")]
    static partial void LogUnexpectedChildElement(ILogger logger, Rune character, string tagName, string parentTagName);

    [LoggerMessage(LogLevel.Warning,
    "`{Character}`: Unexpected XML text node found in element <{TagName}>: `{Text}`")]
    static partial void LogUnexpectedTextNode(ILogger logger, Rune character, string tagName, string text);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{Character}` <{ParentTagName}> contains more than one <{TagName}> element with value `{Text}`")]
    static partial void LogDuplicate(ILogger logger, Rune character, string parentTagName, string text, string tagName);
}
