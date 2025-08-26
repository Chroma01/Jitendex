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

internal static class JmdictServiceCollection
{
    public static IServiceCollection AddJmdictServices(this IServiceCollection services, FilePaths paths) =>
        services.AddSingleton(provider =>
        {
            var resources = provider.GetRequiredService<Resources>();
            return resources.CreateXmlReader(paths.XmlFile);
        })

        .AddTransient<Service>()
        .AddTransient<ReferenceSequencer>(provider =>
        {
            var resources = provider.GetRequiredService<Resources>();
            var cachedIds = resources.LoadJsonDictionary<int>(paths.XRefCache);
            var logger = provider.GetRequiredService<ILogger<ReferenceSequencer>>();
            return new(cachedIds, logger);
        })

        // Top-level readers.
        .AddTransient<IJmdictReader<NoParent, NoChild>, DocumentTypeReader>()
        .AddTransient<IJmdictReader<NoParent, Entry>, EntryReader>()

        // Global document types.
        .AddSingleton<EntityFactory>()

        // Entry element readers.
        .AddTransient<IJmdictReader<Entry, KanjiForm>, KanjiFormReader>()
        .AddTransient<IJmdictReader<Entry, Reading>, ReadingReader>()
        .AddTransient<IJmdictReader<Entry, Sense>, SenseReader>()

        // Kanji form element readers.
        .AddTransient<IJmdictReader<KanjiForm, KInfo>, KInfoReader>()
        .AddTransient<IJmdictReader<KanjiForm, KPriority>, KPriorityReader>()

        // Reading element readers.
        .AddTransient<IJmdictReader<Reading, RInfo>, RInfoReader>()
        .AddTransient<IJmdictReader<Reading, RPriority>, RPriorityReader>()

        // Sense element readers.
        .AddTransient<IJmdictReader<Sense, CrossReference?>, CrossReferenceReader>()
        .AddTransient<IJmdictReader<Sense, Dialect>, DialectReader>()
        .AddTransient<IJmdictReader<Sense, Example>, ExampleReader>()
        .AddTransient<IJmdictReader<Sense, Field>, FieldReader>()
        .AddTransient<IJmdictReader<Sense, Gloss>, GlossReader>()
        .AddTransient<IJmdictReader<Sense, LanguageSource>, LanguageSourceReader>()
        .AddTransient<IJmdictReader<Sense, Misc>, MiscReader>()
        .AddTransient<IJmdictReader<Sense, PartOfSpeech>, PartOfSpeechReader>();
}
