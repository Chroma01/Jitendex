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
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.Models;

public record VocabEntry(string KanjiFormText, string ReadingText, bool IsName = false)
{
    private ImmutableList<Rune>? _kanjiFormRunes;

    public ImmutableList<Rune> KanjiFormRunes()
    {
        if (_kanjiFormRunes is not null)
            return _kanjiFormRunes;

        var textElements = new List<Rune>();
        foreach (var rune in KanjiFormText.EnumerateRunes())
        {
            textElements.Add(rune);
        }
        _kanjiFormRunes = textElements.ToImmutableList();
        return _kanjiFormRunes;
    }

    public override string ToString()
    {
        return $"{KanjiFormText}{Separator.FileField}{ReadingText}";
    }
}
