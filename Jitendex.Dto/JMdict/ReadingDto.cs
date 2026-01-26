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

public sealed class ReadingDto
{
    public required string Text { get; init; }
    public required bool NoKanji { get; init; }
    public List<string> Infos { get; init; } = [];
    public List<string> Priorities { get; init; } = [];
    public List<string> Restrictions { get; init; } = [];

    public override string ToString()
    {
        var sb = new StringBuilder(Text);
        if (Infos.Count > 0)
        {
            sb.Append($"[{string.Join(",", Infos)}]");
        }
        if (Priorities.Count > 0)
        {
            sb.Append($"[{string.Join(",", Priorities)}]");
        }
        if (Restrictions.Count > 0)
        {
            sb.Append($"[{string.Join("；", Restrictions)}]");
        }
        if (NoKanji)
        {
            sb.Append("[nokanji]");
        }
        return sb.ToString();
    }
}
