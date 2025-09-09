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

namespace Jitendex.Furigana.InputModels;

public class VocabEntry
{
    public string KanjiFormText { get; }
    public string ReadingText { get; }
    public bool IsName { get; }
    internal ImmutableArray<Rune> RawKanjiFormRunes { get; }
    internal ImmutableArray<Rune> KanjiFormRunes { get; }

    public VocabEntry(string kanjiFormText, string readingText, bool isName = false)
    {
        KanjiFormText = kanjiFormText;
        ReadingText = readingText;
        IsName = isName;
        RawKanjiFormRunes = [.. kanjiFormText.EnumerateRunes()];
        KanjiFormRunes = [.. CreateKanjiFormRunes(RawKanjiFormRunes)];
    }

    private static readonly ImmutableHashSet<int> _kanjiIterationCharacters = ['々', '〻'];

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

    private static HashSet<int> GetRepeaterIndices(ImmutableArray<Rune> rawRunes)
    {
        var repeaterIndices = new HashSet<int>();
        for (int i = 0; i < rawRunes.Length; i++)
        {
            if (_kanjiIterationCharacters.Contains(rawRunes[i].Value))
                repeaterIndices.Add(i);
        }
        return repeaterIndices;
    }

    public override bool Equals(object? obj) =>
        obj is VocabEntry entry &&
        KanjiFormText == entry.KanjiFormText &&
        ReadingText == entry.ReadingText &&
        IsName == entry.IsName;

    public override int GetHashCode() =>
        HashCode.Combine(KanjiFormText, ReadingText, IsName);
}
