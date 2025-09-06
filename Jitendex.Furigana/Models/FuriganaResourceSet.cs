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

using System.Text;

namespace Jitendex.Furigana.Models;

public class FuriganaResourceSet
{
    private readonly Dictionary<Rune, Kanji> _kanjiDictionary = [];
    private readonly Dictionary<string, SpecialExpression> _specialExpressions = [];

    public FuriganaResourceSet() : this([], []) { }

    public FuriganaResourceSet(IEnumerable<Kanji> kanji) : this(kanji, []) { }

    public FuriganaResourceSet(IEnumerable<Kanji> kanji, IEnumerable<SpecialExpression> specialExpressions)
    {
        _kanjiDictionary = kanji
            .Select(x => new KeyValuePair<Rune, Kanji>(x.Character, x))
            .ToDictionary();

        _specialExpressions = specialExpressions
            .Select(x => new KeyValuePair<string, SpecialExpression>(x.KanjiFormText, x))
            .ToDictionary();
    }

    public Kanji? GetKanji(Rune c)
    {
        if (_kanjiDictionary.TryGetValue(c, out Kanji? kanji))
            return kanji;
        else
            return null;
    }

    public SpecialExpression? GetExpression(string s)
    {
        if (_specialExpressions.TryGetValue(s, out SpecialExpression? exp))
            return exp;
        else
            return null;
    }
}
