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

using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Solvers;

internal class SolutionBuilder
{
    public required List<Solution.Part> Parts;
    public bool IsInitial;

    public string ReadingText() =>
        new(Parts.SelectMany(static x => x.Furigana ?? x.BaseText).ToArray());

    public int ReadingTextLength() => Parts.Aggregate
    (
        seed: 0,
        func: static (sum, part) => sum + (part.Furigana?.Length ?? part.BaseText.Length)
    );

    public string NormalizedReadingText() => ReadingText().KatakanaToHiragana();

    /// <summary>
    /// Merge consecutive parts together if they have null furigana.
    /// </summary>
    public List<Solution.Part> GetNormalizedParts()
    {
        var parts = new List<Solution.Part>();
        var mergedTexts = new List<string>();
        foreach (var part in Parts)
        {
            if (part.Furigana is null)
            {
                mergedTexts.Add(part.BaseText);
                continue;
            }
            if (mergedTexts.Count > 0)
            {
                var baseText = string.Join(string.Empty, mergedTexts);
                parts.Add(new Solution.Part(baseText, null));
                mergedTexts = new List<string>();
            }
            parts.Add(part);
        }
        if (mergedTexts.Count > 0)
        {
            var baseText = string.Join(string.Empty, mergedTexts);
            parts.Add(new Solution.Part(baseText, null));
        }
        return parts;
    }

    public List<IndexedFurigana> GetIndexedParts()
    {
        var indexedParts = new List<IndexedFurigana>();
        int index = 0;
        foreach (var part in Parts)
        {
            int length = part.BaseText.EnumerateRunes().Count();
            if (part.Furigana is not null)
            {
                indexedParts.Add(new IndexedFurigana(part.Furigana, index, index + length - 1));
            }
            index += length;
        }
        return indexedParts;
    }
}