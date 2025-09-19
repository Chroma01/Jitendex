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
using Jitendex.Furigana.Models.TextUnits;

namespace Jitendex.Furigana.Solver;

internal class ResourceCache
{
    public FrozenDictionary<int, JapaneseCharacter> Characters { get; }
    public FrozenDictionary<string, JapaneseCompound> Compounds { get; }

    public ResourceCache(IEnumerable<JapaneseCharacter> characters, IEnumerable<JapaneseCompound> compounds)
    {
        Characters = characters
            .Select(x => new KeyValuePair<int, JapaneseCharacter>(x.Rune.Value, x))
            .ToFrozenDictionary();

        Compounds = compounds
            .Select(x => new KeyValuePair<string, JapaneseCompound>(x.Text, x))
            .ToFrozenDictionary();
    }
}
