/*
Copyright (c) 2025 Stephen Kraus

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms
of the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

namespace Jitendex.Tatoeba.ImportDto;

internal sealed class Document
{
    public DateOnly Date { get; init; }
    public HashSet<int> Sequences { get; init; }
    public Dictionary<int, EnglishSequence> EnglishSequences { get; init; }
    public Dictionary<int, JapaneseSequence> JapaneseSequences { get; init; }
    public Dictionary<(int, int), TokenizedSentence> TokenizedSentences { get; init; }
    public Dictionary<(int, int, int), Token> Tokens { get; init; }

    public Document(DateOnly date, int expectedSequenceCount = 0)
    {
        Date = date;
        Sequences = new(expectedSequenceCount);
        EnglishSequences = new(expectedSequenceCount / 2);
        JapaneseSequences = new(expectedSequenceCount / 2);
        TokenizedSentences = new(expectedSequenceCount / 2);
        Tokens = new(expectedSequenceCount * 4);
    }

    public int NextTokenizedSentenceIndex(int id)
    {
        int index = 0;
        while (TokenizedSentences.ContainsKey((id, index)))
        {
            index++;
        }
        return index;
    }

    public int NextTokenIndex((int, int) id)
    {
        int index = 0;
        while (Tokens.ContainsKey((id.Item1, id.Item2, index)))
        {
            index++;
        }
        return index;
    }

    public IEnumerable<Sequence> GetSequences()
        => Sequences.Select(id => new Sequence { Id = id, CreatedDate = Date });

    public DocumentMetadata GetMetadata()
        => new() { Date = Date };

    public IEnumerable<int> ConcatAllSequenceIds()
        => Sequences
            .Concat(EnglishSequences.Keys)
            .Concat(JapaneseSequences.Keys)
            .Concat(TokenizedSentences.Keys.Select(static key => key.Item1))
            .Concat(Tokens.Keys.Select(static key => key.Item1));
}
