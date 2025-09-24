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
using Microsoft.Extensions.DependencyInjection;

using Jitendex.Import.Jmdict.Readers;
using Jitendex.Import.Jmdict.Readers.DocumentTypes;
using Jitendex.Import.Jmdict.Readers.EntryElementReaders;
using Jitendex.Import.Jmdict.Readers.EntryElementReaders.KanjiFormElementReaders;
using Jitendex.Import.Jmdict.Readers.EntryElementReaders.ReadingElementReaders;
using Jitendex.Import.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

namespace Jitendex.Import.Jmdict;

internal record FilePaths(string XmlFile, string XRefCache);

internal static class JmdictServiceProvider
{
    public static JmdictReader GetService(FilePaths paths) => new ServiceCollection()
        .AddLogging(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }))

        // Global XML file reader.
        .AddTransient<Resources>()
        .AddSingleton(provider =>
        {
            var resources = provider.GetRequiredService<Resources>();
            return resources.CreateXmlReader(paths.XmlFile);
        })

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
        .AddTransient<ReferenceSequencer>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ReferenceSequencer>>();
            var resources = provider.GetRequiredService<Resources>();
            var cachedIds = resources.LoadJsonDictionary<int>(paths.XRefCache);
            return new(logger, cachedIds);
        })

        // Build and return the Jmdict service.
        .AddTransient<JmdictReader>()
        .BuildServiceProvider()
        .GetRequiredService<JmdictReader>();
}
