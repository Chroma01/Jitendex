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

using System.Collections.Frozen;
using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Jitendex.JMdict.Readers;
using Jitendex.JMdict.Readers.DocumentTypes;
using Jitendex.JMdict.Readers.EntryElementReaders;
using Jitendex.JMdict.Readers.EntryElementReaders.KanjiFormElementReaders;
using Jitendex.JMdict.Readers.EntryElementReaders.ReadingElementReaders;
using Jitendex.JMdict.Readers.EntryElementReaders.SenseElementReaders;

namespace Jitendex.JMdict;

internal record FilePaths
{
    public required string Jmdict { get; init; }
    public required string XRefCache { get; init; }
}

internal static class JmdictServiceProvider
{
    public static async Task<JmdictReader> GetServiceAsync(FilePaths paths)
    {
        var xmlReader = CreateXmlReader(paths.Jmdict);
        var cachedIds = await LoadCachedIds(paths.XRefCache);

        return GetService(xmlReader, cachedIds);
    }

    private static XmlReader CreateXmlReader(string path)
    {
        var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var readerSettings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Parse,
            MaxCharactersFromEntities = long.MaxValue,
            MaxCharactersInDocument = long.MaxValue,
        };
        return XmlReader.Create(fileStream, readerSettings);
    }

    private static async Task<FrozenDictionary<string, int>> LoadCachedIds(string path)
    {
        await using var stream = File.OpenRead(path);
        var dictionary = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(stream) ?? [];
        return dictionary.ToFrozenDictionary();
    }

    private static JmdictReader GetService(XmlReader xmlReader, FrozenDictionary<string, int> cachedIds) => new ServiceCollection()
        .AddLogging(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }))

        // Global XML file reader.
        .AddSingleton<XmlReader>(provider => xmlReader)

        // Global document types.
        .AddSingleton<CorpusCache>()
        .AddSingleton<ExampleCache>()
        .AddSingleton<KeywordCache>()

        // Top-level readers.
        .AddTransient<DocumentTypesReader>()
        .AddTransient<EntriesReader>()
        .AddTransient<EntryReader>()

        // Entry element readers.
        .AddTransient<KanjiFormReader>()
        .AddTransient<ReadingReader>()
        .AddTransient<SenseReader>()

        // Kanji form element readers.
        .AddTransient<KInfoReader>()
        .AddTransient<KPriorityReader>()

        // Reading element readers.
        .AddTransient<RestrictionReader>()
        .AddTransient<RInfoReader>()
        .AddTransient<RPriorityReader>()

        // Sense element readers.
        .AddTransient<CrossReferenceReader>()
        .AddTransient<DialectReader>()
        .AddTransient<ExampleReader>()
        .AddTransient<FieldReader>()
        .AddTransient<GlossReader>()
        .AddTransient<KanjiFormRestrictionReader>()
        .AddTransient<LanguageSourceReader>()
        .AddTransient<MiscReader>()
        .AddTransient<PartOfSpeechReader>()
        .AddTransient<ReadingRestrictionReader>()

        // Post-processing of entries.
        .AddSingleton(provider => cachedIds)
        .AddTransient<ReferenceSequencer>()

        // Build and return the Jmdict service.
        .AddTransient<JmdictReader>()
        .BuildServiceProvider()
        .GetRequiredService<JmdictReader>();
}
