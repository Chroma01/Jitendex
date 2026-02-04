/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using Microsoft.Extensions.Logging;
using Jitendex.Tatoeba.Import.Models;

namespace Jitendex.Tatoeba.Import.Parsing;

internal sealed class TatoebaReader
{
    private readonly ILogger<TatoebaReader> _logger;
    private readonly StreamReader _reader;

    public TatoebaReader(ILogger<TatoebaReader> logger, StreamReader reader) =>
        (_logger, _reader) =
        (@logger, @reader);

    public async Task<Document> ReadAsync(DateOnly date)
    {
        Document document = new(date);

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
        var example = GetExample(text, document);
        var translation = GetTranslation(text, document);
        var index = document.NextSegmentationIndex(example.Id);

        var segmentation = new SegmentationElement(example.Id, index, translation.Id);

        var key = segmentation.GetKey();
        document.Segmentations.Add(key, segmentation);

        foreach (var range in text.ElementTextRanges())
        {
            var elementText = text.GetElementText(range);
            var token = new TokenElement
            {
                ExampleId = segmentation.ExampleId,
                SegmentationIndex = segmentation.Index,
                Index = document.NextTokenIndex(key),
                Headword = elementText.GetHeadword(),
                Reading = elementText.GetReading(),
                EntryId = elementText.GetEntryId(),
                SenseNumber = elementText.GetSenseNumber(),
                SentenceForm = elementText.GetSentenceForm(),
                IsPriority = elementText.GetIsPriority(),
            };
            document.Tokens.Add(token.GetKey(), token);
        }
    }

    private ExampleElement GetExample(in ExampleText text, Document document)
    {
        var id = text.GetExampleId();

        if (document.Translations.ContainsKey(id))
        {
            _logger.LogWarning("Sequence ID {Id} is used for different language sentences", id);
        }

        var example = new ExampleElement(id, text.GetExampleText());

        if (!document.Examples.TryGetValue(id, out var oldSentence))
        {
            document.Examples.Add(id, example);
        }
        else if (!string.Equals(example.Text, oldSentence.Text, StringComparison.Ordinal))
        {
            _logger.LogWarning("Japanese sentence #{ID} has more than one distinct text", id);
        }

        return example;
    }

    private TranslationElement GetTranslation(in ExampleText text, Document document)
    {
        var id = text.GetTranslationId();

        if (document.Examples.ContainsKey(id))
        {
            _logger.LogWarning("Sequence ID {Id} is used for different language sentences", id);
        }

        var translation = new TranslationElement(id, text.GetTranslationText());

        if (!document.Translations.TryGetValue(id, out var oldSentence))
        {
            document.Translations.Add(id, translation);
        }
        else if (!string.Equals(translation.Text, oldSentence.Text, StringComparison.Ordinal))
        {
            _logger.LogWarning("English sentence #{ID} has more than one distinct text", id);
        }

        return translation;
    }
}
