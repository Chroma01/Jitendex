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
using Jitendex.Tatoeba.Dto;

namespace Jitendex.Tatoeba.Readers;

internal sealed class TatoebaReader
{
    private readonly ILogger<TatoebaReader> _logger;
    private readonly StreamReader _reader;

    public TatoebaReader(ILogger<TatoebaReader> logger, StreamReader reader) =>
        (_logger, _reader) =
        (@logger, @reader);

    public async Task<Document> ReadAsync(DateOnly date)
    {
        Document document = new(date, expectedSequenceCount: 300_000);

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
                MakeIndex(text, document);
            }
        }

        return document;
    }

    private void MakeIndex(in ExampleText text, Document document)
    {
        var japaneseSequence = GetJapaneseSequence(text, document);
        var englishSequence = GetEnglishSequence(text, document);

        var sentenceTokenization = new TokenizedSentence
        {
            JapaneseSequenceId = japaneseSequence.Id,
            Id = document.NextTokenizedSentenceIndex(japaneseSequence.Id),
            EnglishSequenceId = englishSequence.Id,
        };

        var key = sentenceTokenization.Key;
        document.TokenizedSentences.Add(key, sentenceTokenization);

        foreach (var range in text.ElementTextRanges())
        {
            var elementText = text.GetElementText(range);
            var token = new Token
            {
                SequenceId = sentenceTokenization.JapaneseSequenceId,
                SentenceId = sentenceTokenization.Id,
                Id = document.NextTokenIndex(key),
                Headword = elementText.GetHeadword(),
                Reading = elementText.GetReading(),
                EntryId = elementText.GetEntryId(),
                SenseNumber = elementText.GetSenseNumber(),
                SentenceForm = elementText.GetSentenceForm(),
                IsPriority = elementText.GetIsPriority(),
            };
            document.Tokens.Add(token.Key, token);
        }
    }

    private JapaneseSequence GetJapaneseSequence(in ExampleText text, Document document)
    {
        var id = text.GetJapaneseSentenceId();

        if (document.EnglishSequences.ContainsKey(id))
        {
            _logger.LogWarning("Sequence ID {Id} is used for different language sentences", id);
        }

        var sentence = new JapaneseSequence
        {
            Id = id,
            Text = text.GetJapaneseSentenceText(),
        };

        if (!document.JapaneseSequences.TryGetValue(id, out var oldSentence))
        {
            document.Sequences.Add(id);
            document.JapaneseSequences.Add(id, sentence);
        }
        else if (!string.Equals(sentence.Text, oldSentence.Text, StringComparison.Ordinal))
        {
            _logger.LogWarning("Japanese sentence #{ID} has more than one distinct text", id);
        }

        return sentence;
    }

    private EnglishSequence GetEnglishSequence(in ExampleText text, Document document)
    {
        var id = text.GetEnglishSentenceId();

        if (document.JapaneseSequences.ContainsKey(id))
        {
            _logger.LogWarning("Sequence ID {Id} is used for different language sentences", id);
        }

        var sentence = new EnglishSequence
        {
            Id = id,
            Text = text.GetEnglishSentenceText()
        };

        if (!document.EnglishSequences.TryGetValue(id, out var oldSentence))
        {
            document.Sequences.Add(id);
            document.EnglishSequences.Add(id, sentence);
        }
        else if (!string.Equals(sentence.Text, oldSentence.Text, StringComparison.Ordinal))
        {
            _logger.LogWarning("English sentence #{ID} has more than one distinct text", id);
        }

        return sentence;
    }
}
