/*
Copyright (c) 2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

namespace Jitendex.Dto.JMdict;

public sealed record LanguageSourceDto(string? Text, string LanguageCode, string TypeName, bool IsWasei)
{
    public override string ToString()
    {
        var sb = new StringBuilder(LanguageCode);
        if (!string.Equals(TypeName, "full", StringComparison.Ordinal))
        {
            sb.Append($" ({TypeName})");
        }
        if (IsWasei)
        {
            sb.Append(" [wasei]");
        }
        if (Text is not null)
        {
            sb.Append($": {Text}");
        }
        return sb.ToString();
    }
}
