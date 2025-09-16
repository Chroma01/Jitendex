/*
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
using DataDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.IEnumerable<string>>;

namespace Jitendex.Furigana.Test;

internal static class ResourceMethods
{
    public static IEnumerable<JapaneseCharacter> VocabKanji(DataDictionary dataDictionary) => dataDictionary
        .Select(static item => new VocabKanji
        (
            item.Key.EnumerateRunes().First(),
            item.Value
        ));

    public static IEnumerable<JapaneseCharacter> NameKanji(DataDictionary dataDictionary) => dataDictionary
        .Select(static item => new NameKanji
        (
            item.Key.EnumerateRunes().First(),
            item.Value
        ));

    public static IEnumerable<SpecialExpression> SpecialExpressions(DataDictionary dataDictionary) => dataDictionary
        .Select(static item => new SpecialExpression
        (
            item.Key,
            item.Value
        ));
}
