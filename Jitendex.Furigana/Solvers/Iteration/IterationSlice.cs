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
using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.InputModels;

namespace Jitendex.Furigana.Solvers.Iteration;

internal class IterationSlice
{
    private readonly Entry _entry;
    private readonly int _sliceStart;
    private readonly int _sliceEnd;

    public ImmutableArray<Rune> PriorKanjiFormRunes { get => _entry.KanjiFormRunes[.._sliceStart]; }
    public ImmutableArray<Rune> KanjiFormRunes { get => _entry.KanjiFormRunes[_sliceStart.._sliceEnd]; }
    public ImmutableArray<Rune> RemainingKanjiFormRunes { get => _entry.KanjiFormRunes[_sliceEnd..]; }

    public ImmutableArray<Rune> RawKanjiFormRunes { get => _entry.RawKanjiFormRunes[_sliceStart.._sliceEnd]; }

    public Rune PreviousKanjiFormRune() => PriorKanjiFormRunes.LastOrDefault();
    public Rune NextKanjiFormRune() => RemainingKanjiFormRunes.FirstOrDefault();

    public bool ContainsFirstRune { get => _sliceStart == 0; }
    public bool ContainsFinalRune { get => _sliceEnd == _entry.KanjiFormRunes.Length; }

    public string KanjiFormText() => string.Join(string.Empty, KanjiFormRunes);
    public string RawKanjiFormText() => string.Join(string.Empty, RawKanjiFormRunes);
    public string RemainingKanjiFormText() => string.Join(string.Empty, RemainingKanjiFormRunes);
    public string RemainingKanjiFormTextNormalized() => RemainingKanjiFormText().KatakanaToHiragana();

    public IterationSlice(Entry entry, int sliceStart, int sliceEnd)
    {
        _entry = entry;
        _sliceStart = sliceStart;
        _sliceEnd = sliceEnd;
    }
}
