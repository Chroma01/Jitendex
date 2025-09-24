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

using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Import.Jmdict.Models;
using Jitendex.Import.Jmdict.Models.EntryElements;
using Jitendex.Import.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Import.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal partial class ExampleReader : IJmdictReader<Sense, Example>
{
    private readonly ILogger<ExampleReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly ExampleCache _exampleCache;

    public ExampleReader(ILogger<ExampleReader> logger, XmlReader xmlReader, ExampleCache exampleCache) =>
        (_logger, _xmlReader, _exampleCache) =
        (@logger, @xmlReader, @exampleCache);

    public async Task ReadAsync(Sense sense)
    {
        var example = new Example
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.Examples.Count + 1,
            SourceTypeName = string.Empty,
            SourceKey = default,
            Keyword = string.Empty,
            Sense = sense,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(example);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, Example.XmlTagName, text);
                    sense.Entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Example.XmlTagName;
                    break;
            }
        }

        if (example.SourceKey == default)
        {
            sense.Entry.IsCorrupt = true;
        }
        else
        {
            sense.Examples.Add(example);
        }
    }

    private async Task ReadChildElementAsync(Example example)
    {
        switch (_xmlReader.Name)
        {
            case ExampleSource.XmlTagName:
                await ReadExampleSource(example);
                break;
            case ExampleSource.XmlTagName_Keyword:
                example.Keyword = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            case ExampleSource.XmlTagName_Sentence:
                await ReadExampleSentence(example);
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, Example.XmlTagName);
                example.Sense.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadExampleSource(Example example)
    {
        if (example.SourceTypeName != string.Empty || example.SourceKey != default)
        {
            LogKeyRedefinition(example.EntryId, example.SenseOrder, example.Order, ExampleSource.XmlTagName);
            example.Sense.Entry.IsCorrupt = true;
        }

        var sourceTypeName = _xmlReader.GetAttribute("exsrc_type");

        if (sourceTypeName is not null)
        {
            example.SourceTypeName = sourceTypeName;
        }
        else
        {
            LogMissingSourceType(example.EntryId, example.SenseOrder, example.Order);
            example.Sense.Entry.IsCorrupt = true;
            example.SourceTypeName = Guid.NewGuid().ToString();
        }

        var sourceText = await _xmlReader.ReadElementContentAsStringAsync();

        if (int.TryParse(sourceText, out int sourceKey))
        {
            example.SourceKey = sourceKey;
            example.Source = _exampleCache.GetExampleSource(example.SourceTypeName, example.SourceKey);
        }
        else
        {
            LogInvalidSourceKey(example.EntryId, example.SenseOrder, example.Order, sourceText);
            example.Sense.Entry.IsCorrupt = true;
        }
    }

    private async Task ReadExampleSentence(Example example)
    {
        var sentenceLanguage = _xmlReader.GetAttribute("xml:lang");
        switch (sentenceLanguage)
        {
            case ExampleSource.XmlTagName_Sentence_Japanese:
                await ReadJapaneseSentence(example);
                break;
            case ExampleSource.XmlTagName_Sentence_English:
                await ReadEnglishSentence(example);
                break;
            default:
                LogUnexpectedLanguage(example.EntryId, example.SenseOrder, example.Order, sentenceLanguage);
                example.Sense.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadJapaneseSentence(Example example)
    {
        var text = await _xmlReader.ReadElementContentAsStringAsync();

        if (example.Source.Text != string.Empty && example.Source.Text != text)
        {
            LogMultipleExamples(example.SourceTypeName, example.SourceKey);
            example.Sense.Entry.IsCorrupt = true;
        }

        example.Source.Text = text;
    }

    private async Task ReadEnglishSentence(Example example)
    {
        var translation = await _xmlReader.ReadElementContentAsStringAsync();

        if (example.Source.Translation != string.Empty && example.Source.Translation != translation)
        {
            LogMultipleTranslations(example.SourceTypeName, example.SourceKey);
            example.Sense.Entry.IsCorrupt = true;
        }

        example.Source.Translation = translation;
    }

    [LoggerMessage(LogLevel.Error,
    "Entry `{EntryId}` sense #{SenseOrder} example #{ExampleOrder} has a non-numeric source key: `{Key}`")]
    private partial void LogInvalidSourceKey(int entryId, int senseOrder, int exampleOrder, string key);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` sense #{SenseOrder} example #{ExampleOrder} contains multiple <{ExampleSourceTagName}> elements")]
    private partial void LogKeyRedefinition(int entryId, int senseOrder, int exampleOrder, string exampleSourceTagName);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` sense #{SenseOrder} example #{ExampleOrder} has no source type attribute")]
    private partial void LogMissingSourceType(int entryId, int senseOrder, int exampleOrder);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` sense #{SenseOrder} example #{ExampleOrder} has a sentence in an unexpected language: `{Language}`")]
    private partial void LogUnexpectedLanguage(int entryId, int senseOrder, int exampleOrder, string? language);

    [LoggerMessage(LogLevel.Warning,
    "Example source `{SourceTypeName}` key `{SourceKey}` has more than one text.")]
    private partial void LogMultipleExamples(string sourceTypeName, int sourceKey);

    [LoggerMessage(LogLevel.Warning,
    "Example source `{SourceTypeName}` key `{SourceKey}` has more than one translation.")]
    private partial void LogMultipleTranslations(string sourceTypeName, int sourceKey);
}
