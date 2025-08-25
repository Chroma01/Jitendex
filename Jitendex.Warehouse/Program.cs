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

        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                }))
            .AddSingleton<XmlReader>(provider =>
            {
                var path = Path.Combine("Resources", "edrdg", "JMdict_e_examp");
                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var readerSettings = new XmlReaderSettings
                {
                    Async = true,
                    DtdProcessing = DtdProcessing.Parse,
                    MaxCharactersFromEntities = long.MaxValue,
                    MaxCharactersInDocument = long.MaxValue,
                };
                return XmlReader.Create(fileStream, readerSettings);
            })
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
            .AddTransient<Jmdict.Readers.EntryElementReaders.SenseElementReaders.PartOfSpeechReader>()

            .AddTransient<Jmdict.Readers.EntryElementReaders.KanjiFormReader>()
            .AddTransient<Jmdict.Readers.EntryElementReaders.ReadingReader>()
            .AddTransient<Jmdict.Readers.EntryElementReaders.SenseReader>()

            .AddTransient<Jmdict.Readers.EntryReader>()

            .AddSingleton<Jmdict.EntityFactory>()
            .AddTransient<Jmdict.ReferenceSequencer>()
            .AddTransient<Jmdict.Reader>()

            .AddTransient<Resources>()

            .BuildServiceProvider();


        var jmdictReader = serviceProvider.GetRequiredService<Jmdict.Reader>();
        await jmdictReader.ReadEntriesAsync();

        sw.Stop();
        Console.WriteLine($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
    }
}
