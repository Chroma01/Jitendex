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
using Jitendex.KanjiVG.Models;
using Jitendex.KanjiVG.Readers;
using Jitendex.KanjiVG.Readers.Metadata;

namespace Jitendex.KanjiVG;

internal record Files
{
    public required FileInfo SvgArchive { get; init; }
}

internal static class ReaderProvider
{
    public static KanjiVGReader GetReader(Files paths) => new ServiceCollection()
        .AddLogging(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }))

        // XML file resource.
        .AddSingleton<Files>(provider => paths)
        .AddTransient<KanjiFiles>()

        // Global metadata
        .AddSingleton<ComponentGroupStyleCache>()
        .AddSingleton<StrokeNumberGroupStyleCache>()

        // Top-level readers.
        .AddTransient<EntriesReader>()
        .AddTransient<EntryReader>()

        // Stroke Path Components
        .AddTransient<ComponentGroupReader>()
        .AddTransient<ComponentAttributesReader>()
        .AddTransient<ComponentReader>()
        .AddTransient<StrokeReader>()

        // Stroke Numbers
        .AddTransient<StrokeNumberGroupReader>()
        .AddTransient<StrokeNumberReader>()

        // Build and return the KanjiVG service.
        .AddTransient<KanjiVGReader>()
        .BuildServiceProvider()
        .GetRequiredService<KanjiVGReader>();
}
