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

namespace Jitendex.Chise.Models.Sequences;

public sealed class SurroundFromRightSequence : Sequence, ISequence
{
    internal const char Indicator = '⿼';
    static string ISequence.GetIndicator() => Indicator.ToString();
    static int ISequence.ArgumentCount() => 2;
    static string ISequence.FirstPositionName() => "RightSurrounding";
    static string ISequence.SecondPositionName() => "LeftSurrounded";
    static string ISequence.ThirdPositionName() => throw new NotImplementedException();
}
