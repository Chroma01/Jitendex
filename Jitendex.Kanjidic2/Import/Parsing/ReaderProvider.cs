/*
Copyright (c) 2025 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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
using Jitendex.Kanjidic2.Import.Parsing.GroupReaders;

namespace Jitendex.Kanjidic2.Import.Parsing;

internal static class ReaderProvider
{
    public static Kanjidic2Reader GetReader(FileInfo file) => new ServiceCollection()
        .AddLogging(static builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }))

        // Global XML file reader.
        .AddSingleton(_ =>
            {
                var readerSettings = new XmlReaderSettings
                {
                    Async = true,
                    DtdProcessing = DtdProcessing.Parse,
                    MaxCharactersFromEntities = long.MaxValue,
                    MaxCharactersInDocument = long.MaxValue,
                };
                FileStream f = new(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                BrotliStream b = new(f, CompressionMode.Decompress);
                return XmlReader.Create(b, readerSettings);
            })

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
}
