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

using Jitendex.Kanjidic2.Models.EntryElements;

namespace Jitendex.Kanjidic2.Models;

internal sealed class Document
{
    public required string Date { get; set; }

    #region Keywords
    public Dictionary<string, CodepointType> CodepointTypes { get; init; } = [];
    public Dictionary<string, DictionaryType> DictionaryTypes { get; init; } = [];
    public Dictionary<string, QueryCodeType> QueryCodeTypes { get; init; } = [];
    public Dictionary<string, MisclassificationType> MisclassificationTypes { get; init; } = [];
    public Dictionary<string, RadicalType> RadicalTypes { get; init; } = [];
    public Dictionary<string, ReadingType> ReadingTypes { get; init; } = [];
    public Dictionary<string, VariantType> VariantTypes { get; init; } = [];
    #endregion

    #region Entry data
    private const int EntriesCount = 13_200;
    public Dictionary<int, Entry> Entries { get; init; } = new(EntriesCount);
    public Dictionary<(int, int), Codepoint> Codepoints { get; init; } = new(EntriesCount * 3);
    public Dictionary<(int, int), Dictionary> Dictionaries { get; init; } = new(EntriesCount * 6);
    public Dictionary<(int, int), Meaning> Meanings { get; init; } = new(EntriesCount * 2);
    public Dictionary<(int, int), Nanori> Nanoris { get; init; } = new(EntriesCount / 2);
    public Dictionary<(int, int), QueryCode> QueryCodes { get; init; } = new(EntriesCount * 3);
    public Dictionary<(int, int), Radical> Radicals { get; init; } = new(EntriesCount * 2);
    public Dictionary<(int, int), RadicalName> RadicalNames { get; init; } = new(200);
    public Dictionary<(int, int), Reading> Readings { get; init; } = new(EntriesCount * 7);
    public Dictionary<(int, int), StrokeCount> StrokeCounts { get; init; } = new(EntriesCount * 2);
    public Dictionary<(int, int), Variant> Variants { get; init; } = new(EntriesCount / 2);
    #endregion
}
