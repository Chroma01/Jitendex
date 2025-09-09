/*
Copyright (c) 2015, 2017 Doublevil
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

using System.Collections.Frozen;
using System.Text;

namespace Jitendex.Furigana.InputModels;

public class ResourceSet
{
    private readonly FrozenDictionary<(int, bool), Kanji> _kanjiDictionary;
    private readonly FrozenDictionary<string, SpecialExpression> _specialExpressions;

    public ResourceSet(IEnumerable<Kanji> kanji, IEnumerable<SpecialExpression> specialExpressions)
    {
        _kanjiDictionary = kanji
            .Select(x => new KeyValuePair<(int, bool), Kanji>((x.Character.Value, x is NameKanji), x))
            .ToFrozenDictionary();

        _specialExpressions = specialExpressions
            .Select(x => new KeyValuePair<string, SpecialExpression>(x.Expression, x))
            .ToFrozenDictionary();
    }

    public Kanji? GetKanji(Rune character, Entry entry)
    {
        if (_kanjiDictionary.TryGetValue((character.Value, entry is NameEntry), out Kanji? kanji))
            return kanji;
        else
            return null;
    }

    public SpecialExpression? GetExpression(string text)
    {
        if (_specialExpressions.TryGetValue(text, out SpecialExpression? expression))
            return expression;
        else
            return null;
    }
}
