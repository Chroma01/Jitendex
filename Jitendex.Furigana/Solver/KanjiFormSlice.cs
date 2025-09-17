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

using System.Collections.Immutable;
using System.Text;
using Jitendex.Furigana.Models;
using Jitendex.Furigana.TextExtensions;

namespace Jitendex.Furigana.Solver;

internal class KanjiFormSlice
{
    private readonly Entry _entry;
    private readonly int _sliceStart;
    private readonly int _sliceEnd;

    public ImmutableArray<Rune> PriorRunes { get => _entry.NormalizedKanjiFormRunes[.._sliceStart]; }
    public ImmutableArray<Rune> Runes { get => _entry.NormalizedKanjiFormRunes[_sliceStart.._sliceEnd]; }
    public ImmutableArray<Rune> RemainingRunes { get => _entry.NormalizedKanjiFormRunes[_sliceEnd..]; }

    public ImmutableArray<Rune> RawRunes { get => _entry.KanjiFormRunes[_sliceStart.._sliceEnd]; }

    public Rune PreviousRune() => PriorRunes.LastOrDefault();
    public Rune NextRune() => RemainingRunes.FirstOrDefault();

    public bool ContainsFirstRune { get => _sliceStart == 0; }
    public bool ContainsFinalRune { get => _sliceEnd == _entry.NormalizedKanjiFormRunes.Length; }

    public string Text() => string.Join(string.Empty, Runes);
    public string RawText() => string.Join(string.Empty, RawRunes);
    public string RemainingText() => string.Join(string.Empty, RemainingRunes);

    public KanjiFormSlice(Entry entry, int sliceStart, int sliceEnd)
    {
        _entry = entry;
        _sliceStart = sliceStart;
        _sliceEnd = sliceEnd;
    }
}
