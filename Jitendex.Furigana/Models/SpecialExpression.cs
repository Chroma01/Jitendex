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

namespace Jitendex.Furigana.Models;

/// <summary>
/// Represents a special reading expression.
/// For example, 大人 - おとな can't be cut as おと.な or お.とな.
/// These readings are loaded from the SpecialReadings.txt file.
/// </summary>
public class SpecialExpression(string kanjiReading, List<SpecialReading> readings)
{
    public string KanjiReading { get; set; } = kanjiReading;
    public List<SpecialReading> Readings { get; set; } = readings;

    public SpecialExpression() : this(string.Empty, new List<SpecialReading>())
    {

    }

    public SpecialExpression(string kanjiReading, params SpecialReading[] readings) : this(kanjiReading, readings.ToList())
    {

    }
}
