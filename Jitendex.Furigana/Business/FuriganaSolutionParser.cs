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

using Jitendex.Furigana.Models;
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.Business;

public static class FuriganaSolutionParser
{
    public static FuriganaSolution? Parse(string s, VocabEntry v)
    {
        var parts = new List<FuriganaPart>();
        var partSplit = s.Split(SeparatorHelper.MultiValueSeparator);

        foreach (var partString in partSplit)
        {
            var fieldSeparator = partString.Split(SeparatorHelper.AssociationSeparator);
            if (fieldSeparator.Length != 2)
            {
                // Malformed input or just a simple reading.
                // Treat it like a simple reading.
                parts.Add(new FuriganaPart(partString, 0, v.KanjiFormText.Length));
                continue;
            }

            var indexesString = fieldSeparator[0];
            var furiganaValue = fieldSeparator[1];

            int? minIndex, maxIndex;

            var indexSplit = indexesString.Split(SeparatorHelper.RangeSeparator);
            if (indexSplit.Length == 2)
            {
                minIndex = int.TryParse(indexSplit[0], out int x) ? x : null;
                maxIndex = int.TryParse(indexSplit[1], out int y) ? y : null;
            }
            else if (indexSplit.Length == 1)
            {
                minIndex = int.TryParse(indexSplit[0], out int x) ? x : null;
                maxIndex = minIndex;
            }
            else
            {
                // Malformed input.
                return null;
            }

            if (minIndex.HasValue && maxIndex.HasValue && minIndex.Value <= maxIndex.Value)
            {
                parts.Add(new FuriganaPart(furiganaValue, minIndex.Value, maxIndex.Value));
            }
            else
            {
                // Malformed input.
                return null;
            }
        }

        // Everything went fine. Return the solution.
        return new FuriganaSolution(v, parts);
    }
}
