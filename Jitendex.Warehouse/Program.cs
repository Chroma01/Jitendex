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

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Jitendex.Warehouse;

public class Program
{
    public static async Task Main()
    {
        var sw = new Stopwatch();
        sw.Start();

        var serviceCollection = new ServiceCollection()
            .AddLogging(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                }))
            .AddTransient<Resources>();

        var jmdictPath = new Extensions.JmdictPath
        (
            XmlFile: Path.Combine("Resources", "edrdg", "JMdict_e_examp"),
            XRefCache: Path.Combine("Resources", "jmdict", "cross_reference_sequences.json")
        );
        serviceCollection.AddJmdictServices(jmdictPath);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        var jmdictReader = serviceProvider.GetRequiredService<Jmdict.Reader>();
        await jmdictReader.ReadEntriesAsync();

        sw.Stop();
        logger.LogInformation($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
    }
}

internal static class Extensions
{
    public record JmdictPath(string XmlFile, string XRefCache);

    public static IServiceCollection AddJmdictServices(this IServiceCollection services, JmdictPath jmdictPath) => services
        .AddSingleton(provider =>
        {
            var resources = provider.GetRequiredService<Resources>();
            return resources.CreateXmlReader(jmdictPath.XmlFile);
        })
        .AddTransient<Jmdict.Reader>()
        .AddSingleton<Jmdict.EntityFactory>()
        .AddTransient<Jmdict.ReferenceSequencer>(provider =>
        {
            var resources = provider.GetRequiredService<Resources>();
            var cachedIds = resources.LoadJsonDictionary<int>(jmdictPath.XRefCache);
            var logger = provider.GetRequiredService<ILogger<Jmdict.ReferenceSequencer>>();
            return new(cachedIds, logger);
        })
        .AddTransient<Jmdict.Readers.EntryReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.KanjiFormReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.ReadingReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.SenseReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.KanjiFormElementReaders.InfoReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.KanjiFormElementReaders.PriorityReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.ReadingElementReaders.InfoReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.ReadingElementReaders.PriorityReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.SenseElementReaders.CrossReferenceReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.SenseElementReaders.DialectReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.SenseElementReaders.ExampleReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.SenseElementReaders.FieldReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.SenseElementReaders.GlossReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.SenseElementReaders.LanguageSourceReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.SenseElementReaders.MiscReader>()
        .AddTransient<Jmdict.Readers.EntryElementReaders.SenseElementReaders.PartOfSpeechReader>();
}
