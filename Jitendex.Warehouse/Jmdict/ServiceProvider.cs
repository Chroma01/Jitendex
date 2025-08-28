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

using Jitendex.Warehouse.Jmdict.Models;
using Jitendex.Warehouse.Jmdict.Models.EntryElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.KanjiFormElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.ReadingElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

using Jitendex.Warehouse.Jmdict.Readers;
using Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders;
using Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.KanjiFormElementReaders;
using Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.ReadingElementReaders;
using Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

namespace Jitendex.Warehouse.Jmdict;

internal record FilePaths(string XmlFile, string XRefCache);

internal static class JmdictServiceProvider
{
    public static Service GetService(FilePaths paths) => new ServiceCollection()
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
        .AddSingleton<DocumentTypes>()

        // Top-level readers.
        .AddTransient<IJmdictReader<NoParent, NoChild>, DocumentTypesReader>()
        .AddTransient<IJmdictReader<List<Entry>, Entry>, EntryReader>()

        // Entry element readers.
        .AddTransient<IJmdictReader<Entry, KanjiForm>, KanjiFormReader>()
        .AddTransient<IJmdictReader<Entry, Reading>, ReadingReader>()
        .AddTransient<IJmdictReader<Entry, Sense>, SenseReader>()

        // Kanji form element readers.
        .AddTransient<IJmdictReader<KanjiForm, KanjiFormInfo>, KInfoReader>()
        .AddTransient<IJmdictReader<KanjiForm, KanjiFormPriority>, KPriorityReader>()

        // Reading element readers.
        .AddTransient<IJmdictReader<Reading, Restriction>, RestrictionReader>()
        .AddTransient<IJmdictReader<Reading, ReadingInfo>, RInfoReader>()
        .AddTransient<IJmdictReader<Reading, ReadingPriority>, RPriorityReader>()

        // Sense element readers.
        .AddTransient<IJmdictReader<Sense, CrossReference>, CrossReferenceReader>()
        .AddTransient<IJmdictReader<Sense, Dialect>, DialectReader>()
        .AddTransient<IJmdictReader<Sense, Example>, ExampleReader>()
        .AddTransient<IJmdictReader<Sense, Field>, FieldReader>()
        .AddTransient<IJmdictReader<Sense, Gloss>, GlossReader>()
        .AddTransient<IJmdictReader<Sense, KanjiFormRestriction>, KanjiFormRestrictionReader>()
        .AddTransient<IJmdictReader<Sense, LanguageSource>, LanguageSourceReader>()
        .AddTransient<IJmdictReader<Sense, Misc>, MiscReader>()
        .AddTransient<IJmdictReader<Sense, PartOfSpeech>, PartOfSpeechReader>()
        .AddTransient<IJmdictReader<Sense, ReadingRestriction>, ReadingRestrictionReader>()

        // Post-processing of entries.
        .AddTransient<ReferenceSequencer>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ReferenceSequencer>>();
            var resources = provider.GetRequiredService<Resources>();
            var cachedIds = resources.LoadJsonDictionary<int>(paths.XRefCache);
            return new(logger, cachedIds);
        })

        // Build and return the Jmdict service.
        .AddTransient<Service>()
        .BuildServiceProvider()
        .GetRequiredService<Service>();
}
