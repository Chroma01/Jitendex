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

namespace Jitendex.Tatoeba.Models;

internal sealed class Document
{
    public DateOnly Date { get; init; }
    public Dictionary<int, Sequence> Sequences { get; init; }
    public Dictionary<int, EnglishSentence> EnglishSentences { get; init; }
    public Dictionary<int, JapaneseSentence> JapaneseSentences { get; init; }
    public Dictionary<(int, int), SentenceIndex> SentenceIndices { get; init; }
    public Dictionary<(int, int, int), IndexElement> IndexElements { get; init; }

    public Document(DateOnly date, int expectedSequenceCount = 0)
    {
        Date = date;
        Sequences = new(expectedSequenceCount);
        EnglishSentences = new(expectedSequenceCount / 2);
        JapaneseSentences = new(expectedSequenceCount / 2);
        SentenceIndices = new(expectedSequenceCount / 2);
        IndexElements = new(expectedSequenceCount * 4);
    }
}
