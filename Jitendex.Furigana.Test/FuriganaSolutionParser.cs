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

using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Test;

public static class FuriganaSolutionParser
{
    private const char MultiValueSeparator = ';';
    private const char AssociationSeparator = ':';
    private const char RangeSeparator = '-';

    public static Solution Parse(string s, VocabEntry v)
    {
        var parts = new List<IndexedFurigana>();
        var partSplit = s.Split(MultiValueSeparator);

        foreach (var partString in partSplit)
        {
            var fieldSeparator = partString.Split(AssociationSeparator);

            if (fieldSeparator.Length != 2)
            {
                throw new Exception($"partstring `{partString}` should contain one and only one separator `{AssociationSeparator}`");
            }

            var indexesString = fieldSeparator[0];
            var furiganaValue = fieldSeparator[1];

            int minIndex, maxIndex;

            var indexSplit = indexesString.Split(RangeSeparator);
            if (indexSplit.Length == 2)
            {
                minIndex = int.Parse(indexSplit[0]);
                maxIndex = int.Parse(indexSplit[1]);
            }
            else if (indexSplit.Length == 1)
            {
                minIndex = int.Parse(indexSplit[0]);
                maxIndex = minIndex;
            }
            else
            {
                throw new Exception($"Malformed input `{indexesString}`");
            }

            parts.Add(new IndexedFurigana(furiganaValue, minIndex, maxIndex));
        }

        return new IndexedSolution(v, parts).ToTextSolution();
    }
}
