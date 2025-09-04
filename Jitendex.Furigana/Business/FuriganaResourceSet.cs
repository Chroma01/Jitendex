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

namespace Jitendex.Furigana.Business;

public class FuriganaResourceSet
{
    private readonly Dictionary<char, Kanji> _kanjiDictionary = [];
    private readonly Dictionary<string, SpecialExpression> _specialExpressions = [];

    public FuriganaResourceSet(
        Dictionary<char, Kanji> kanjiDictionary,
        Dictionary<string, SpecialExpression> specialExpressions)
    {
        _kanjiDictionary = kanjiDictionary;
        _specialExpressions = specialExpressions;
    }

    public Kanji? GetKanji(char c)
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
