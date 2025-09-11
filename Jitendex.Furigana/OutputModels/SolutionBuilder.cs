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
using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.InputModels;
using Jitendex.Furigana.OutputModels;

namespace Jitendex.Furigana.Solvers;

internal class SolutionBuilder
{
    public ImmutableArray<Solution.Part> Parts { get => _parts.ToImmutableArray(); }
    private readonly List<Solution.Part> _parts;

    public SolutionBuilder(IEnumerable<Solution.Part> parts)
    {
        _parts = [.. parts.Where(x => !string.IsNullOrWhiteSpace(x.BaseText))];
    }

    public string KanjiFormText() => new(_parts.SelectMany(static x => x.BaseText).ToArray());
    public string ReadingText() => new(_parts.SelectMany(static x => x.Furigana ?? x.BaseText).ToArray());
    public string NormalizedReadingText() => ReadingText().KatakanaToHiragana();

    public void Add(Solution.Part part)
    {
        if (string.IsNullOrWhiteSpace(part.BaseText))
        {
            throw new ArgumentOutOfRangeException(nameof(part));
        }
        _parts.Add(part);
    }

    public Solution? ToSolution(Entry entry)
    {
        if (!IsValid(entry))
        {
            return null;
        }
        return new Solution
        {
            Entry = entry,
            Parts = [.. GetNormalizedParts()]
        };
    }

    public IndexedSolution? ToIndexedSolution(Entry entry)
    {
        if (!IsValid(entry))
        {
            return null;
        }
        return new IndexedSolution
        (
            entry: entry,
            parts: [.. GetIndexedParts()]
        );
    }

    private bool IsValid(Entry entry) =>
        entry.NormalizedReadingText == NormalizedReadingText() &&
        entry.KanjiFormText == KanjiFormText();

    /// <summary>
    /// Merge consecutive parts together if they have null furigana.
    /// </summary>
    private List<Solution.Part> GetNormalizedParts()
    {
        var parts = new List<Solution.Part>();
        var mergedTexts = new List<string>();
        foreach (var part in Parts)
        {
            if (part.Furigana is null)
            {
                mergedTexts.Add(part.BaseText);
                continue;
            }
            if (mergedTexts.Count > 0)
            {
                var baseText = string.Join(string.Empty, mergedTexts);
                parts.Add(new Solution.Part(baseText, null));
                mergedTexts = new List<string>();
            }
            parts.Add(part);
        }
        if (mergedTexts.Count > 0)
        {
            var baseText = string.Join(string.Empty, mergedTexts);
            parts.Add(new Solution.Part(baseText, null));
        }
        return parts;
    }

    private List<IndexedFurigana> GetIndexedParts()
    {
        var indexedParts = new List<IndexedFurigana>();
        int index = 0;
        foreach (var part in Parts)
        {
            int length = part.BaseText.EnumerateRunes().Count();
            if (part.Furigana is not null)
            {
                indexedParts.Add(new IndexedFurigana(part.Furigana, index, index + length - 1));
            }
            index += length;
        }
        return indexedParts;
    }
}