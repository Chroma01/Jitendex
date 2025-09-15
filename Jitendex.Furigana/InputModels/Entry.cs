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
using System.Text;
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.InputModels;

public abstract class Entry
{
    public string KanjiFormText { get; }
    public string ReadingText { get; }
    internal string NormalizedReadingText { get; }
    internal ImmutableArray<Rune> RawKanjiFormRunes { get; }
    internal ImmutableArray<Rune> KanjiFormRunes { get; }

    public Entry(string kanjiFormText, string readingText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kanjiFormText);
        ArgumentException.ThrowIfNullOrWhiteSpace(readingText);

        if (kanjiFormText.All(KanaComparison.IsKana))
        {
            throw new ArgumentException(
                "Kanji form text must contain at least one non-kana character to solve",
                nameof(kanjiFormText));
        }
        if (readingText.Any(char.IsSurrogate))
        {
            throw new ArgumentException(
                "Reading text must not contain characters with surrogate code units.",
                nameof(readingText));
        }

        KanjiFormText = kanjiFormText;
        ReadingText = readingText;
        NormalizedReadingText = readingText.KatakanaToHiragana();
        RawKanjiFormRunes = [.. kanjiFormText.EnumerateRunes()];
        KanjiFormRunes = [.. CreateKanjiFormRunes(RawKanjiFormRunes)];
    }

    private static bool IsKanjiIterationCharacter(int c) => c switch
    {
        '々' or '〻' => true,
        _ => false
    };

    private static Rune[] CreateKanjiFormRunes(ImmutableArray<Rune> rawRunes)
    {
        var runes = new Rune[rawRunes.Length];
        var repeaterIndices = GetRepeaterIndices(rawRunes);

        // Replace repeaters (々) and double repeaters (々々) with their respective kanji.
        for (int i = 0; i < rawRunes.Length; i++)
        {
            if (i > 1 && repeaterIndices.Contains(i) && repeaterIndices.Contains(i + 1))
            {
                // Double repeater
                runes[i] = runes[i - 2];
                i++;
                runes[i] = runes[i - 2];
            }
            else if (i > 0 && repeaterIndices.Contains(i))
            {
                // Single repeater
                runes[i] = runes[i - 1];
            }
            else
            {
                // No repeater
                runes[i] = rawRunes[i];
            }
        }
        return runes;
    }

    private static ImmutableHashSet<int> GetRepeaterIndices(ImmutableArray<Rune> rawRunes) => rawRunes
        .Select(static (rune, index) =>
        (
            IsRepeater: IsKanjiIterationCharacter(rune.Value),
            Index: index
        ))
        .Where(static x => x.IsRepeater)
        .Select(static x => x.Index)
        .ToImmutableHashSet();

    public override abstract bool Equals(object? obj);

    public override abstract int GetHashCode();

    public override string ToString()
    {
        return $"{ReadingText}【{KanjiFormText}】";
    }
}
