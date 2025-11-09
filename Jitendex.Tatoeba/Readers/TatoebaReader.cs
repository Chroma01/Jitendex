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
    private readonly Dictionary<int, JapaneseSentence> _japaneseSentences = [];
    private readonly Dictionary<int, EnglishSentence> _englishSentences = [];

    public TatoebaReader(ILogger<TatoebaReader> logger, StreamReader reader) =>
        (_logger, _reader) =
        (@logger, @reader);

    public async Task<List<Example>> ReadAsync()
    {
        var examples = new List<Example>(150_000);

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
                    var example = MakeExample(text);
                    examples.Add(example);
                }
                catch (FormatException e)
                {
                    _logger.LogError("Error parsing example text: \"{Message}\"", e.Message);
                }
            }
        }

        return examples;
    }

    private Example MakeExample(in ExampleText text)
    {
        var japaneseSentence = GetJapaneseSentence(text);
        var englishSentence = GetEnglishSentence(text);

        var index = new SentenceIndex
        {
            SentenceId = japaneseSentence.Id,
            Order = japaneseSentence.Indices.Count + 1,
            Sentence = japaneseSentence,
        };

        japaneseSentence.Indices.Add(index);

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
        }

        var example = new Example
        {
            JapaneseSentenceId = japaneseSentence.Id,
            EnglishSentenceId = englishSentence.Id,
            JapaneseSentence = japaneseSentence,
            EnglishSentence = englishSentence,
        };

        japaneseSentence.Examples.Add(example);
        englishSentence.Examples.Add(example);

        return example;
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
            return sentence;
        }
    }
}
