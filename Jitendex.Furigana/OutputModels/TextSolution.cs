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

namespace Jitendex.Furigana.OutputModels;

public class TextSolution
{
    public record Part(string BaseText, string? Furigana);

    public VocabEntry Vocab { get; set; }
    public List<Part> Parts { get; set; }

    public TextSolution(IndexedSolution indexedSolution)
    {
        Vocab = indexedSolution.Vocab;
        Parts = MakeParts(indexedSolution);
    }

    private static List<Part> MakeParts(IndexedSolution indexedSolution)
    {
        var parts = new List<Part>();
        var runes = indexedSolution.Vocab.RawKanjiFormRunes();
        int? kanaStart = null;

        for (int i = 0; i < runes.Count; i++)
        {
            var matchingFurigana = indexedSolution.Parts
                .FirstOrDefault(f => f.StartIndex == i);

            if (matchingFurigana is not null)
            {
                // We are on a furigana start index.
                // If there was any kana, output that part first
                if (kanaStart.HasValue)
                {
                    parts.Add(new Part(
                        BaseText: string.Join("", runes.ToArray()[kanaStart.Value..i]),
                        Furigana: null));
                    kanaStart = null;
                }

                // Then output the furigana part
                parts.Add(new Part(
                    BaseText: string.Join("", runes.GetRange(i, matchingFurigana.EndIndex - i + 1)),
                    Furigana: matchingFurigana.Value));

                i = matchingFurigana.EndIndex;
            }
            else
            {
                // We are not on a furigana-covered character, must be kana. Set kanaStart if not already set.
                kanaStart ??= i;
            }
        }

        // Output the final kana part if any
        if (kanaStart.HasValue)
        {
            parts.Add(new Part(
                BaseText: string.Join("", runes.ToArray()[kanaStart.Value..]),
                Furigana: null));
        }
        return parts;
    }
}
