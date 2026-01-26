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

public sealed class EntryDto
{
    public List<KanjiFormDto> KanjiForms { get; init; } = [];
    public List<ReadingDto> Readings { get; init; } = [];
    public List<SenseDto> Senses { get; init; } = [];

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (KanjiForms.Count > 0)
        {
            sb.AppendLine("Kanji Forms");
            for (int i = 0; i < KanjiForms.Count; i++)
            {
                sb.AppendLine($"\t{i + 1}: {KanjiForms[i]}");
            }
        }
        sb.AppendLine("Readings");
        for (int i = 0; i < Readings.Count; i++)
        {
            sb.AppendLine($"\t{i + 1}: {Readings[i]}");
        }
        sb.AppendLine("Senses");
        for (int i = 0; i < Senses.Count; i++)
        {
            sb.AppendLine($"\t{i + 1}. {Senses[i]}");
        }
        return sb.ToString();
    }
}
