/*
Copyright (c) 2025 Doublevil
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

using System.Globalization;

namespace Jitendex.Furigana.Helpers;

public static class ParsingHelper
{
    private static readonly NumberStyles NumberStyles = NumberStyles.Number;

    public static readonly CultureInfo DefaultCulture =
        CultureInfo.CreateSpecificCulture("en-US");

    public static int? ParseInt(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        if (int.TryParse(input, NumberStyles, DefaultCulture, out int output))
        {
            return output;
        }

        return null;
    }
}
