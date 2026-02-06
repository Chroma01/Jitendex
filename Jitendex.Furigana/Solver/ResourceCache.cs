/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Frozen;
using System.Text;
using Jitendex.Furigana.Models.TextUnits;
using Jitendex.Furigana.Models.TextUnits.Readings;

namespace Jitendex.Furigana.Solver;

internal sealed class ResourceCache
{
    public FrozenDictionary<int, JapaneseCharacter> Characters { get; }
    public FrozenDictionary<string, JapaneseCompound> Compounds { get; }

    public ResourceCache(IEnumerable<JapaneseCharacter> characters, IEnumerable<JapaneseCompound> compounds)
    {
        Characters = characters
            .Select(static x => new KeyValuePair<int, JapaneseCharacter>(x.Rune.Value, x))
            .ToFrozenDictionary();

        Compounds = compounds
            .Select(static x => new KeyValuePair<string, JapaneseCompound>(x.Text, x))
            .ToFrozenDictionary();
    }

    public UnknownCharacterReading NewReading(Rune rune, string readingText)
    {
        if (Characters.TryGetValue(rune.Value, out JapaneseCharacter? character))
        {
            var reading = new UnknownCharacterReading(character, readingText);
            return reading;
        }
        else
        {
            var newCharacter = new Kanji(rune, [], []);
            var reading = new UnknownCharacterReading(newCharacter, readingText);
            return reading;
        }
    }
}
