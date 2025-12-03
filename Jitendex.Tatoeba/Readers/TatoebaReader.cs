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

using Microsoft.Extensions.Logging;
using Jitendex.Tatoeba.Models;

namespace Jitendex.Tatoeba.Readers;

internal sealed class TatoebaReader
{
    private readonly ILogger<TatoebaReader> _logger;
    private readonly StreamReader _reader;

    public TatoebaReader(ILogger<TatoebaReader> logger, StreamReader reader) =>
        (_logger, _reader) =
        (@logger, @reader);

    public async Task<Document> ReadAsync()
    {
        Document document = new(initialCapacity: 150_000);

        while (await _reader.ReadLineAsync() is string lineA)
        {
            if (!lineA.StartsWith("A: ", StringComparison.Ordinal))
            {
                _logger.LogError("Expected `{LineA}` to start with \"A: \"", lineA);
            }
            else if (await _reader.ReadLineAsync() is not string lineB)
            {
                _logger.LogError("No B-line found for A-line `{LineA}`", lineA);
            }
            else if (!lineB.StartsWith("B: ", StringComparison.Ordinal))
            {
                _logger.LogError("Expected `{LineB}` to start with \"B: \"", lineB);
            }
            else
            {
                var text = new ExampleText(lineA.AsSpan(3), lineB.AsSpan(3));
                try
                {
                    MakeIndex(text, document);
                }
                catch (Exception e)
                {
                    using (_logger.BeginScope((lineA, lineB)))
                        _logger.LogError("Error parsing example text: \"{Message}\"", e.Message);
                }
            }
        }

        return document;
    }

    private void MakeIndex(in ExampleText text, Document document)
    {
        var japaneseSentence = GetJapaneseSentence(text, document);
        var order = japaneseSentence.Indices.Count + 1;
        var englishSentence = GetEnglishSentence(text, document);

        var index = new SentenceIndex
        {
            SentenceId = japaneseSentence.SequenceId,
            Order = order,
            MeaningId = englishSentence.SequenceId,
            Sentence = japaneseSentence,
            Meaning = englishSentence,
        };

        japaneseSentence.Indices.Add(index);
        englishSentence.Indices.Add(index);

        foreach (var range in text.ElementTextRanges())
        {
            var elementText = text.GetElementText(range);
            var indexElement = new IndexElement
            {
                SentenceId = index.SentenceId,
                IndexOrder = index.Order,
                Order = index.Elements.Count + 1,
                Headword = elementText.GetHeadword(),
                Reading = elementText.GetReading(),
                EntryId = elementText.GetEntryId(),
                SenseNumber = elementText.GetSenseNumber(),
                SentenceForm = elementText.GetSentenceForm(),
                IsPriority = elementText.GetIsPriority(),
                Index = index,
            };
            index.Elements.Add(indexElement);
            document.IndexElements.Add(indexElement.Key, indexElement);
        }

        document.SentenceIndices.Add(index.Key, index);
    }

    private JapaneseSentence GetJapaneseSentence(in ExampleText text, Document document)
    {
        var sequence = new Sequence
        {
            Id = text.GetJapaneseSentenceId(),
        };

        if (document.Sequences.TryGetValue(sequence.Id, out var oldSequence))
        {
            sequence = oldSequence;
        }
        else
        {
            document.Sequences.Add(sequence.Id, sequence);
        }

        if (sequence.EnglishSentence is not null)
        {
            _logger.LogWarning("Sequence ID {Id} is used for different language sentences", sequence.Id);
        }

        var sentence = new JapaneseSentence
        {
            SequenceId = sequence.Id,
            Text = text.GetJapaneseSentenceText(),
            Sequence = sequence,
        };

        if (sequence.JapaneseSentence is null)
        {
            sequence.JapaneseSentence = sentence;
            document.JapaneseSentences.Add(sequence.Id, sentence);
        }
        else if (!string.Equals(sentence.Text, sequence.JapaneseSentence.Text, StringComparison.Ordinal))
        {
            _logger.LogWarning("Japanese sentence #{ID} has more than one distinct text", sentence.SequenceId);
        }

        return sequence.JapaneseSentence;
    }

    private EnglishSentence GetEnglishSentence(in ExampleText text, Document document)
    {
        var sequence = new Sequence
        {
            Id = text.GetEnglishSentenceId(),
        };

        if (document.Sequences.TryGetValue(sequence.Id, out var oldSequence))
        {
            sequence = oldSequence;
        }
        else
        {
            document.Sequences.Add(sequence.Id, sequence);
        }

        if (sequence.JapaneseSentence is not null)
        {
            _logger.LogWarning("Sequence ID {Id} is used for different language sentences", sequence.Id);
        }

        var sentence = new EnglishSentence
        {
            SequenceId = sequence.Id,
            Text = text.GetEnglishSentenceText(),
            Sequence = sequence,
        };

        if (sequence.EnglishSentence is null)
        {
            sequence.EnglishSentence = sentence;
            document.EnglishSentences.Add(sequence.Id, sentence);
        }
        else if (!string.Equals(sentence.Text, sequence.EnglishSentence.Text, StringComparison.Ordinal))
        {
            _logger.LogWarning("English sentence #{ID} has more than one distinct text", sentence.SequenceId);
        }

        return sequence.EnglishSentence;
    }
}
