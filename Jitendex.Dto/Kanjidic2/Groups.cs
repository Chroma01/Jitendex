/*
Copyright (c) 2026 Stephen Kraus
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

namespace Jitendex.Dto.Kanjidic2;

public sealed class CodepointGroupDto
{
    public List<CodepointDto> Codepoints { get; init; } = [];
}

public sealed class DictionaryGroupDto
{
    public List<DictionaryDto> Dictionaries { get; init; } = [];
}

public sealed class MiscGroupDto
{
    public int? Grade { get; set; }
    public int? Frequency { get; set; }
    public int? JlptLevel { get; set; }
    public List<string> RadicalNames { get; init; } = [];
    public List<int> StrokeCounts { get; init; } = [];
    public List<VariantDto> Variants { get; init; } = [];
}

public sealed class QueryCodeGroupDto
{
    public List<QueryCodeDto> QueryCodes { get; init; } = [];
}

public sealed class RadicalGroupDto
{
    public List<RadicalDto> Radicals { get; init; } = [];
}

public sealed class ReadingMeaningGroupDto
{
    public List<ReadingMeaningDto> ReadingMeanings { get; init; } = [];
    public List<string> Nanoris { get; init; } = [];
}

public sealed class ReadingMeaningDto
{
    public bool IsKokuji { get; set; }
    public bool IsGhost { get; set; }
    public List<string> Meanings { get; init; } = [];
    public List<ReadingDto> Readings { get; init; } = [];
}
