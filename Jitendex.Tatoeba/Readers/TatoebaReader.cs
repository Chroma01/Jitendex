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

internal class TatoebaReader
{
    private readonly ILogger<TatoebaReader> _logger;
    private readonly StreamReader _reader;

    private const int _capacity = 150_000;
    private readonly Dictionary<int, JapaneseSentence> _japaneseSentences = new(_capacity);
    private readonly Dictionary<int, EnglishSentence> _englishSentences = new(_capacity);
    private readonly Dictionary<(int, int), int> _indexOrder = new(_capacity);

    public TatoebaReader(ILogger<TatoebaReader> logger, StreamReader reader) =>
        (_logger, _reader) =
        (@logger, @reader);

    public async Task<List<SentenceIndex>> ReadAsync()
    {
        var indices = new List<SentenceIndex>(_capacity);

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
                var text = new ExampleText(lineA.AsSpan()[3..], lineB.AsSpan()[3..]);
                try
                {
                    var index = MakeIndex(text);
                    indices.Add(index);
                }
                catch (FormatException e)
                {
                    _logger.LogError("Error parsing example text: \"{Message}\"", e.Message);
                }
            }
        }

        return indices;
    }

    private SentenceIndex MakeIndex(in ExampleText text)
    {
        var japaneseSentence = GetJapaneseSentence(text);
        var englishSentence = GetEnglishSentence(text);
        var order = GetOrder(japaneseSentence.Id, englishSentence.Id);

        var index = new SentenceIndex
        {
            SentenceId = japaneseSentence.Id,
            MeaningId = englishSentence.Id,
            Order = order,
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
                MeaningId = index.MeaningId,
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
        }

        return index;
    }

    private JapaneseSentence GetJapaneseSentence(in ExampleText text)
    {
        var sentence = new JapaneseSentence
        {
            Id = text.GetJapaneseSentenceId(),
            Text = text.GetJapaneseSentenceText(),
        };

        if (_japaneseSentences.TryGetValue(sentence.Id, out var oldSentence))
        {
            if (!string.Equals(sentence.Text, oldSentence.Text, StringComparison.Ordinal))
            {
                _logger.LogWarning("Sentence ID #{ID} has more than one distinct text", sentence.Id);
            }
            return oldSentence;
        }
        else
        {
            _japaneseSentences[sentence.Id] = sentence;
            return sentence;
        }
    }

    private EnglishSentence GetEnglishSentence(in ExampleText text)
    {
        var sentence = new EnglishSentence
        {
            Id = text.GetEnglishSentenceId(),
            Text = text.GetEnglishSentenceText(),
        };

        if (_englishSentences.TryGetValue(sentence.Id, out var oldSentence))
        {
            if (!string.Equals(sentence.Text, oldSentence.Text, StringComparison.Ordinal))
            {
                _logger.LogWarning("Sentence ID #{ID} has more than one distinct text", sentence.Id);
            }
            return oldSentence;
        }
        else
        {
            _englishSentences[sentence.Id] = sentence;
            return sentence;
        }
    }

    private int GetOrder(int sentenceId, int meaningId)
    {
        var key = (sentenceId, meaningId);
        if (!_indexOrder.TryGetValue(key, out int order))
        {
            order = 1;
        }
        _indexOrder[key] = order + 1;
        return order;
    }
}
