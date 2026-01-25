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

namespace Jitendex.Dto.Tatoeba;

public sealed record TokenDto
{
    public required string Headword { get; set; }
    public required string? Reading { get; set; }
    public required int? EntryId { get; set; }
    public required int? SenseNumber { get; set; }
    public required string? SentenceForm { get; set; }
    public required bool IsPriority { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder(Headword);
        if (Reading is not null)
            sb.Append($"({Reading})");
        if (EntryId is not null)
            sb.Append($"(#{EntryId})");
        if (SenseNumber is not null)
            sb.Append($"[{SenseNumber}]");
        if (SentenceForm is not null)
            sb.Append($"{{{SentenceForm}}}");
        if (IsPriority)
            sb.Append('~');
        return sb.ToString();
    }
}
