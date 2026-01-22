/*
Copyright (c) 2025-2026 Stephen Kraus
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

using Jitendex.JMdict.Import.Parsing.EntryElementReaders;
using Jitendex.JMdict.Import.Parsing.EntryElementReaders.KanjiFormElementReaders;
using Jitendex.JMdict.Import.Parsing.EntryElementReaders.ReadingElementReaders;
using Jitendex.JMdict.Import.Parsing.EntryElementReaders.SenseElementReaders;

namespace Jitendex.JMdict.Import.Parsing;

internal static class ReaderProvider
{
    public static JmdictReader GetReader(FileInfo jmdictFile) => new ServiceCollection()
        .AddLogging(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }))

        // Global XML file reader.
        .AddSingleton(provider =>
        {
            var readerSettings = new XmlReaderSettings
            {
                Async = true,
                DtdProcessing = DtdProcessing.Parse,
                MaxCharactersFromEntities = long.MaxValue,
                MaxCharactersInDocument = long.MaxValue,
            };
            FileStream f = new(jmdictFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            BrotliStream b = new(f, CompressionMode.Decompress);
            return XmlReader.Create(b, readerSettings);
        })

        // Top-level readers.
        .AddTransient<DocumentTypeReader>()
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
        .AddTransient<FieldReader>()
        .AddTransient<GlossReader>()
        .AddTransient<KanjiFormRestrictionReader>()
        .AddTransient<LanguageSourceReader>()
        .AddTransient<MiscReader>()
        .AddTransient<PartOfSpeechReader>()
        .AddTransient<ReadingRestrictionReader>()

        // Build and return the Jmdict service.
        .AddTransient<JmdictReader>()
        .BuildServiceProvider()
        .GetRequiredService<JmdictReader>();
}
