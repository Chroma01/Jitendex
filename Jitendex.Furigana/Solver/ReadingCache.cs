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
using System.Text;
using Jitendex.Furigana.Models;

namespace Jitendex.Furigana.Solver;

internal class ReadingCache
{
    private readonly FrozenDictionary<(int, Type), JapaneseCharacter> _japaneseCharacters;
    private readonly FrozenDictionary<string, SpecialExpression> _specialExpressions;

    public ReadingCache(IEnumerable<JapaneseCharacter> japaneseCharacters, IEnumerable<SpecialExpression> specialExpressions)
    {
        _japaneseCharacters = japaneseCharacters
            .Select(x => new KeyValuePair<(int, Type), JapaneseCharacter>((x.Rune.Value, x.GetType()), x))
            .ToFrozenDictionary();

        _specialExpressions = specialExpressions
            .Select(x => new KeyValuePair<string, SpecialExpression>(x.Expression, x))
            .ToFrozenDictionary();
    }

    public ImmutableArray<CharacterReading> GetCharacterReadings(Entry entry, Rune rune)
    {
        if (TryGetCharacter(rune, entry, out JapaneseCharacter japaneseCharacter))
        {
            return japaneseCharacter.Readings;
        }
        else
        {
            return [];
        }
    }

    public ImmutableArray<string> GetSpecialExpressionReadings(string text)
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

    private bool TryGetCharacter(Rune rune, Entry entry, out JapaneseCharacter japaneseCharacter)
    {
        if (entry is NameEntry && _japaneseCharacters.TryGetValue((rune.Value, typeof(NameKanji)), out JapaneseCharacter? nameKanji))
        {
            japaneseCharacter = nameKanji;
            return true;
        }
        if (_japaneseCharacters.TryGetValue((rune.Value, typeof(VocabKanji)), out JapaneseCharacter? vocabKanji))
        {
            japaneseCharacter = vocabKanji;
            return true;
        }
        japaneseCharacter = null!;
        return false;
    }
}
