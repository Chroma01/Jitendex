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

namespace Jitendex.Import.Kanjidic2.Models;

public class Kanjidic2Document
{
    public required List<Entry> Entries { get; init; }
    public required List<CodepointType> CodepointTypes { get; init; }
    public required List<DictionaryType> DictionaryTypes { get; init; }
    public required List<QueryCodeType> QueryCodeTypes { get; init; }
    public required List<MisclassificationType> MisclassificationTypes { get; init; }
    public required List<RadicalType> RadicalTypes { get; init; }
    public required List<ReadingType> ReadingType { get; init; }
    public required List<VariantType> VariantTypes { get; init; }
}
