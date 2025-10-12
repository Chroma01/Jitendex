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

using System.IO.Compression;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Jitendex.Kanjidic2.Readers;
using Jitendex.Kanjidic2.Readers.GroupReaders;

namespace Jitendex.Kanjidic2;

internal record Files
{
    public required FileInfo Kanjidic2 { get; init; }
}

internal static class ReaderProvider
{
    public static Kanjidic2Reader GetReader(Files files) => new ServiceCollection()
        .AddLogging(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }))

        // Global XML file reader.
        .AddSingleton(provider =>
            CreateXmlReader(files.Kanjidic2.FullName))

        // Global document types.
        .AddSingleton<DocumentTypes>()

        // Top-level readers.
        .AddTransient<HeaderReader>()
        .AddTransient<EntriesReader>()
        .AddTransient<EntryReader>()

        // Group readers.
        .AddTransient<CodepointGroupReader>()
        .AddTransient<DictionaryGroupReader>()
        .AddTransient<MiscGroupReader>()
        .AddTransient<QueryCodeGroupReader>()
        .AddTransient<RadicalGroupReader>()
        .AddTransient<ReadingMeaningGroupReader>()

        // Subgroup readers.
        .AddTransient<ReadingMeaningReader>()

        // Build and return the Kanjidic2 service.
        .AddTransient<Kanjidic2Reader>()
        .BuildServiceProvider()
        .GetRequiredService<Kanjidic2Reader>();


    private static XmlReader CreateXmlReader(string path)
    {
        var readerSettings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Parse,
            MaxCharactersFromEntities = long.MaxValue,
            MaxCharactersInDocument = long.MaxValue,
        };
        FileStream f = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        BrotliStream b = new(f, CompressionMode.Decompress);
        return XmlReader.Create(b, readerSettings);
    }
}
