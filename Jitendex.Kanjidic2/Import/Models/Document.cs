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

using Jitendex.Kanjidic2.Import.Models.Groups;
using Jitendex.Kanjidic2.Import.Models.GroupElements;
using Jitendex.Kanjidic2.Import.Models.SubgroupElements;

namespace Jitendex.Kanjidic2.Import.Models;

internal sealed class Document
{
    public required Header Header { get; init; }
    public Dictionary<int, Entry> Entries { get; init; }

    #region Keywords
    public Dictionary<string, CodepointType> CodepointTypes { get; init; } = [];
    public Dictionary<string, DictionaryType> DictionaryTypes { get; init; } = [];
    public Dictionary<string, QueryCodeType> QueryCodeTypes { get; init; } = [];
    public Dictionary<string, MisclassificationType> MisclassificationTypes { get; init; } = [];
    public Dictionary<string, RadicalType> RadicalTypes { get; init; } = [];
    public Dictionary<string, ReadingType> ReadingTypes { get; init; } = [];
    public Dictionary<string, VariantType> VariantTypes { get; init; } = [];
    #endregion

    #region Groups
    public Dictionary<(int, int), CodepointGroup> CodepointGroups { get; init; }
    public Dictionary<(int, int), DictionaryGroup> DictionaryGroups { get; init; }
    public Dictionary<(int, int), MiscGroup> MiscGroups { get; init; }
    public Dictionary<(int, int), QueryCodeGroup> QueryCodeGroups { get; init; }
    public Dictionary<(int, int), RadicalGroup> RadicalGroups { get; init; }
    public Dictionary<(int, int), ReadingMeaningGroup> ReadingMeaningGroups { get; init; }
    #endregion

    #region Group Elements
    public Dictionary<(int, int, int), Codepoint> Codepoints { get; init; }
    public Dictionary<(int, int, int), Dictionary> Dictionaries { get; init; }
    public Dictionary<(int, int, int), Nanori> Nanoris { get; init; }
    public Dictionary<(int, int, int), QueryCode> QueryCodes { get; init; }
    public Dictionary<(int, int, int), Radical> Radicals { get; init; }
    public Dictionary<(int, int, int), RadicalName> RadicalNames { get; init; }
    public Dictionary<(int, int, int), ReadingMeaning> ReadingMeanings { get; init; }
    public Dictionary<(int, int, int), StrokeCount> StrokeCounts { get; init; }
    public Dictionary<(int, int, int), Variant> Variants { get; init; }
    #endregion

    #region Subgroup Elements
    public Dictionary<(int, int, int, int), Meaning> Meanings { get; init; }
    public Dictionary<(int, int, int, int), Reading> Readings { get; init; }
    #endregion

    public Document(int expectedEntryCount = 13_200)
    {
        CodepointTypes = [];
        DictionaryTypes = [];
        QueryCodeTypes = [];
        MisclassificationTypes = [];
        RadicalTypes = [];
        ReadingTypes = [];
        VariantTypes = [];

        Entries = new(expectedEntryCount);

        CodepointGroups = new(expectedEntryCount);
        DictionaryGroups = new(expectedEntryCount);
        MiscGroups = new(expectedEntryCount);
        QueryCodeGroups = new(expectedEntryCount);
        RadicalGroups = new(expectedEntryCount);
        ReadingMeaningGroups = new(expectedEntryCount);

        Codepoints = new(expectedEntryCount * 3);
        Dictionaries = new(expectedEntryCount * 6);
        Nanoris = new(expectedEntryCount / 2);
        QueryCodes = new(expectedEntryCount * 3);
        Radicals = new(expectedEntryCount * 2);
        RadicalNames = new(expectedEntryCount / 66);
        ReadingMeanings = new(expectedEntryCount);
        StrokeCounts = new(expectedEntryCount * 2);
        Variants = new(expectedEntryCount / 2);

        Meanings = new(expectedEntryCount * 2);
        Readings = new(expectedEntryCount * 7);
    }
}
