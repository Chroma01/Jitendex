/*
Copyright (c) 2025 Doublevil
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

using Jitendex.Furigana.Helpers;
using Jitendex.Furigana.InputModels;

namespace Jitendex.Furigana.OutputModels;

/// <summary>
/// A vocab entry with a furigana reading string.
/// </summary>
public class IndexedSolution
{
    public VocabEntry Vocab { get; set; }
    public List<IndexedFurigana> Parts { get; set; }

    public IndexedSolution(VocabEntry vocab, params IndexedFurigana[] parts) : this(vocab, parts.ToList()) { }

    public IndexedSolution(VocabEntry vocab, List<IndexedFurigana> parts)
    {
        Vocab = vocab;
        Parts = parts;
    }

    public bool Check()
    {
        return IndexedSolutionChecker.Check(this);
    }

    public TextSolution ToTextSolution()
    {
        return new TextSolution(this);
    }

    public override string ToString()
    {
        return string.Join
        (
            Separator.FileField,
            new string[]
            {
                Vocab.KanjiFormText,
                Vocab.ReadingText,
                string.Join
                (
                    Separator.MultiValue,
                    Parts.Select(x => x.ToString()).ToArray()
                )
            }
        );
    }

    public override bool Equals(object? obj)
    {
        if (obj is IndexedSolution other)
        {
            // Compare both solutions.
            if (Vocab != other.Vocab || Parts.Count != other.Parts.Count)
            {
                // Not the same vocab or not the same count of furigana parts.
                return false;
            }

            // If there is at least one furigana part that has no equivalent in the other
            // furigana solution, then the readings differ.
            return Parts.All(f1 => other.Parts.Any(f2 => f1.Equals(f2)))
                && other.Parts.All(f2 => Parts.Any(f1 => f1.Equals(f2)));
        }
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
