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

using System.Text;
using System.Text.RegularExpressions;
using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.InputModels;

namespace Jitendex.Furigana.Solvers.Iteration;

internal class ReadingState
{
    private readonly Entry _entry;
    private readonly int _readingIndex;

    public string ReadingText { get => _entry.ReadingText; }
    public string PriorReadingText { get => ReadingText[.._readingIndex]; }
    public string RemainingReadingText { get => ReadingText[_readingIndex..]; }

    public string ReadingTextNormalized { get => _entry.NormalizedReadingText; }
    public string PriorReadingTextNormalized { get => ReadingTextNormalized[.._readingIndex]; }
    public string RemainingReadingTextNormalized { get => ReadingTextNormalized[_readingIndex..]; }

    public ReadingState(Entry entry, int readingIndex)
    {
        _entry = entry;
        _readingIndex = readingIndex;
    }

    public string? MinimumReading() =>
        RemainingReadingText == string.Empty ? null : RemainingReadingTextNormalized[..1];

    public string? RegularKanjiReading()
    {
        if (RemainingReadingText == string.Empty)
        {
            return null;
        }
        var reading = new StringBuilder(RemainingReadingTextNormalized[..1]);
        foreach (var character in RemainingReadingTextNormalized[1..])
        {
            if (IsImpossibleKanjiReadingStart(character))
            {
                reading.Append(character);
            }
            else
            {
                break;
            }
        }
        return reading.ToString();
    }

    private static bool IsImpossibleKanjiReadingStart(char c) => c switch
    {
        'っ' or 'ょ' or 'ゃ' or 'ゅ' or 'ん' => true,
        _ => false
    };

    public string? RegexReading(string remainingKanjiFormTextNormalized)
    {
        var greedyMatch = Match("(.+)", remainingKanjiFormTextNormalized);
        var lazyMatch = Match("(.+?)", remainingKanjiFormTextNormalized);

        if (!greedyMatch.Success || !lazyMatch.Success)
        {
            return null;
        }

        var greedyValue = greedyMatch.Groups[1].Value;

        if (greedyValue != string.Empty && greedyValue == lazyMatch.Groups[1].Value)
        {
            return greedyValue;
        }
        else
        {
            return null;
        }
    }

    private Match Match(string groupPattern, string remainingKanjiFormTextNormalized)
    {
        var pattern = new StringBuilder($"^{groupPattern}");
        foreach (var character in remainingKanjiFormTextNormalized)
        {
            if (character.IsKana())
            {
                pattern.Append(character);
            }
            else
            {
                pattern.Append(groupPattern);
            }
        }
        pattern.Append('$');
        var regex = new Regex(pattern.ToString());
        return regex.Match(RemainingReadingTextNormalized);
    }
}
