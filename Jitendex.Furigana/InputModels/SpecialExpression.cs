/*
Copyright (c) 2015 Doublevil
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

using System.Collections.Immutable;
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.InputModels;

/// <summary>
/// Represents a special reading expression.
/// For example, 大人 - おとな can't be cut as おと.な or お.とな.
/// </summary>
public class SpecialExpression(string expression, IEnumerable<string> readings)
{
    public string Expression { get; } = expression;
    public ImmutableArray<string> Readings { get; } = readings
        .Select(static reading => reading.KatakanaToHiragana())
        .Distinct()
        .ToImmutableArray();
}
