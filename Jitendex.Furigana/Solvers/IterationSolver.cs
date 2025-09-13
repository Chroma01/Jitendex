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

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Solvers;

internal class IterationSolver : FuriganaSolver
{
    private readonly ReadingCache _resourceSet;

    public IterationSolver(ReadingCache resourceSet)
    {
        _resourceSet = resourceSet;
    }

    public override IEnumerable<IndexedSolution> Solve(Entry entry)
    {
        var builders = new List<SolutionBuilder>() { new() };
        for (int sliceStart = 0; sliceStart < entry.KanjiFormRunes.Length; sliceStart++)
        {
            var newBuilders = new List<SolutionBuilder>();
            for (int sliceEnd = entry.KanjiFormRunes.Length; sliceStart < sliceEnd; sliceEnd--)
            {
                var iterationSlice = new IterationSlice(entry, sliceStart, sliceEnd);
                newBuilders = IterateBuilders(builders, iterationSlice);
                if (newBuilders.Count > 0)
                {
                    sliceStart += sliceEnd - sliceStart - 1;
                    builders = newBuilders;
                    break;
                }
            }
            if (newBuilders.Count == 0)
            {
                yield break;
            }
        }
        foreach (var builder in builders)
        {
            var solution = builder.ToIndexedSolution(entry);
            if (solution is not null)
            {
                yield return solution;
            }
        }
    }

    private List<SolutionBuilder> IterateBuilders(List<SolutionBuilder> oldBuilders, IterationSlice iterationSlice)
    {
        var newBuilders = new List<SolutionBuilder>();

        var cachedReadings = _resourceSet.GetPotentialReadings(iterationSlice);
        var baseReadingsMaker = new BaseReadingsMaker(iterationSlice, cachedReadings);

        foreach (var oldBuilder in oldBuilders)
        {
            var baseReadings = baseReadingsMaker.GetBaseReadings(oldBuilder);
            var oldReadingText = oldBuilder.NormalizedReadingText();
            var oldParts = oldBuilder.ToParts();

            foreach (var baseReading in baseReadings)
            {
                if (TryGetNewPart(iterationSlice, oldReadingText, baseReading, out var newPart))
                {
                    newBuilders.Add(new(oldParts.Add(newPart))); // Note: `oldParts` is immutable.
                }
            }
        }
        return newBuilders;
    }

    private static bool TryGetNewPart(IterationSlice iterationSlice, string oldReadingText, string baseReading, [NotNullWhen(returnValue: true)] out Solution.Part? part)
    {
        var baseText = iterationSlice.RawKanjiFormText;
        if (!iterationSlice.Entry.NormalizedReadingText.StartsWith(oldReadingText + baseReading))
        {
            part = null;
            return false;
        }
        else if (baseText.IsKanaEquivalent(baseReading))
        {
            part = new(baseText, null);
            return true;
        }
        else
        {
            // Use the raw, non-normalized reading for the furigana text.
            int i = oldReadingText.Length;
            int j = i + baseReading.Length;
            var furigana = iterationSlice.Entry.ReadingText[i..j];
            part = new(baseText, furigana);
            return true;
        }
    }
}

internal class IterationSlice
{
    public Entry Entry { get; }
    public ImmutableArray<Rune> KanjiFormRunes { get; }
    public ImmutableArray<Rune> RawKanjiFormRunes { get; }
    public string KanjiFormText { get; }
    public string RawKanjiFormText { get; }
    public bool ContainsFirstRune { get; }
    public bool ContainsFinalRune { get; }
    public IterationSlice(Entry entry, int sliceStart, int sliceEnd)
    {
        Entry = entry;
        KanjiFormRunes = entry.KanjiFormRunes[sliceStart..sliceEnd];
        RawKanjiFormRunes = entry.RawKanjiFormRunes[sliceStart..sliceEnd];
        KanjiFormText = string.Join(string.Empty, KanjiFormRunes);
        RawKanjiFormText = string.Join(string.Empty, RawKanjiFormRunes);
        ContainsFirstRune = sliceStart == 0;
        ContainsFinalRune = sliceEnd == entry.KanjiFormRunes.Length;
    }
}

internal class BaseReadingsMaker
{
    private readonly Entry _entry;
    private readonly ImmutableArray<string> _cachedReadings;
    private readonly bool _addDefaultReading;
    private static readonly FrozenSet<char> _impossibleKanjiReadingStart = ['っ', 'ょ', 'ゃ', 'ゅ', 'ん'];

    public BaseReadingsMaker(IterationSlice iterationSlice, ImmutableArray<string> cachedReadings)
    {
        _entry = iterationSlice.Entry;
        _cachedReadings = cachedReadings;

        var baseTextRunes = iterationSlice.RawKanjiFormRunes;
        _addDefaultReading = baseTextRunes.Length == 1 && baseTextRunes.First().IsKanji();
    }

    public ImmutableArray<string> GetBaseReadings(SolutionBuilder builder)
    {
        if (!_addDefaultReading)
        {
            return _cachedReadings;
        }

        var remainingKanjiFormRunes = RemainingKanjiFormRunes(builder);
        var remainingReadingText = RemainingReadingText(builder);

        if (remainingKanjiFormRunes.Length == 0 || string.IsNullOrEmpty(remainingReadingText))
        {
            return _cachedReadings;
        }

        var defaultReading = DefaultReading(remainingKanjiFormRunes, remainingReadingText);

        if (_cachedReadings.Contains(defaultReading))
        {
            return _cachedReadings;
        }
        else
        {
            return _cachedReadings.Add(defaultReading);
        }
    }

    private ImmutableArray<Rune> RemainingKanjiFormRunes(SolutionBuilder builder)
    {
        var builderKanjiFormText = builder.KanjiFormText();
        if (!_entry.KanjiFormText.StartsWith(builderKanjiFormText))
        {
            return [];
        }
        return _entry.KanjiFormRunes[builderKanjiFormText.EnumerateRunes().Count()..];
    }

    private string? RemainingReadingText(SolutionBuilder builder)
    {
        var builderReadingText = builder.NormalizedReadingText();
        if (!_entry.NormalizedReadingText.StartsWith(builderReadingText))
        {
            return null;
        }
        return _entry.NormalizedReadingText[builderReadingText.Length..];
    }

    private static string DefaultReading(ImmutableArray<Rune> remainingKanjiFormRunes, string remainingReadingText)
    {
        var defaultReadingBuilder = new StringBuilder(remainingReadingText[..1]);
        Rune nextRune = remainingKanjiFormRunes.Length > 1 ? remainingKanjiFormRunes[1] : new();
        if (remainingReadingText.Length > 1)
        {
            foreach (var readingCharacter in remainingReadingText[1..])
            {
                if (nextRune.IsKana() && readingCharacter.IsKanaEquivalent((char)nextRune.Value))
                {
                    break;
                }
                else if (_impossibleKanjiReadingStart.Contains(readingCharacter))
                {
                    defaultReadingBuilder.Append(readingCharacter);
                }
                else
                {
                    break;
                }
            }
        }
        return defaultReadingBuilder.ToString();
    }
}
