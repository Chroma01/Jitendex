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

using System.Collections.Immutable;
using System.Text;

namespace Jitendex.Furigana.InputModels;

public record VocabEntry(string KanjiFormText, string ReadingText, bool IsName = false)
{
    private ImmutableList<Rune>? _rawKanjiFormRunes;
    private ImmutableList<Rune>? _kanjiFormRunes;

    internal ImmutableList<Rune> RawKanjiFormRunes()
    {
        if (_rawKanjiFormRunes is not null)
            return _rawKanjiFormRunes;
        _rawKanjiFormRunes = KanjiFormText.EnumerateRunes().ToImmutableList();
        return _rawKanjiFormRunes;
    }

    internal ImmutableList<Rune> KanjiFormRunes()
    {
        if (_kanjiFormRunes is not null)
            return _kanjiFormRunes;

        var runes = new List<Rune>();
        var rawRunes = RawKanjiFormRunes();
        var repeaterIndices = GetRepeaterIndices();

        // Replace repeaters (々) and double repeaters (々々) with their respective kanji.
        for (int i = 0; i < rawRunes.Count; i++)
        {
            if (i > 1 && repeaterIndices.Contains(i) && repeaterIndices.Contains(i + 1))
            {
                // Double repeater
                runes.AddRange(runes[i - 2], runes[i - 1]);
                i++;
            }
            else if (i > 0 && repeaterIndices.Contains(i))
            {
                // Single repeater
                runes.Add(runes[i - 1]);
            }
            else
            {
                // No repeater
                runes.Add(rawRunes[i]);
            }
        }
        _kanjiFormRunes = runes.ToImmutableList();
        return _kanjiFormRunes;
    }

    private HashSet<int> GetRepeaterIndices()
    {
        var repeaterIndices = new HashSet<int>();
        var rawRunes = RawKanjiFormRunes();
        for (int i = 0; i < rawRunes.Count; i++)
        {
            if (rawRunes[i].Value == '々')
                repeaterIndices.Add(i);
        }
        return repeaterIndices;
    }
}
