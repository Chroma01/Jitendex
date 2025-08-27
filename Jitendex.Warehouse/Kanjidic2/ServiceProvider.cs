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

// using Jitendex.Warehouse.Kanjidic2.Models;
// using Jitendex.Warehouse.Kanjidic2.Models.Groups;
// using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

using Jitendex.Warehouse.Kanjidic2.Readers;
using Jitendex.Warehouse.Kanjidic2.Readers.GroupReaders;

namespace Jitendex.Warehouse.Kanjidic2;

internal record FilePaths(string XmlFile);

internal static class Kanjidic2ServiceProvider
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

        // Top-level readers.
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
        .AddTransient<Service>()
        .BuildServiceProvider()
        .GetRequiredService<Service>();
}
