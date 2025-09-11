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
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Jitendex.Furigana.Helpers;

namespace Jitendex.Furigana.InputModels;

public class ResourceSet
{
    private readonly FrozenDictionary<(int, Type), Kanji> _kanjiDictionary;
    private readonly FrozenDictionary<string, SpecialExpression> _specialExpressions;

    public ResourceSet(IEnumerable<Kanji> kanji, IEnumerable<SpecialExpression> specialExpressions)
    {
        _kanjiDictionary = kanji
            .Select(x => new KeyValuePair<(int, Type), Kanji>((x.Character.Value, x.GetType()), x))
            .ToFrozenDictionary();

        _specialExpressions = specialExpressions
            .Select(x => new KeyValuePair<string, SpecialExpression>(x.Expression, x))
            .ToFrozenDictionary();
    }

    public ImmutableArray<string> GetPotentialReadings(Rune character, Entry entry, bool isFirstRune, bool isLastRune)
    {
        if (TryGetKanji(character, entry, out Kanji? kanji))
        {
            return kanji.GetPotentialReadings(isFirstRune, isLastRune);
        }
        else if (character.IsKana())
        {
            // In normal circumstances, a kana's only reading is itself.
            var characterAsHiragana = ((char)character.Value).KatakanaToHiragana();
            return [characterAsHiragana.ToString()];
        }
        else
        {
            return [];
        }
    }

    public ImmutableArray<string> GetPotentialReadings(string text)
    {
        if (_specialExpressions.TryGetValue(text, out SpecialExpression? expression))
        {
            return expression.Readings;
        }
        else
        {
            return [];
        }
    }

    private bool TryGetKanji(Rune character, Entry entry, [NotNullWhen(returnValue: true)] out Kanji? kanji)
    {
        if (entry is NameEntry && _kanjiDictionary.TryGetValue((character.Value, typeof(NameKanji)), out Kanji? nameKanji))
        {
            kanji = nameKanji;
            return true;
        }
        if (_kanjiDictionary.TryGetValue((character.Value, typeof(VocabKanji)), out Kanji? vocabKanji))
        {
            kanji = vocabKanji;
            return true;
        }
        kanji = null;
        return false;
    }
}
