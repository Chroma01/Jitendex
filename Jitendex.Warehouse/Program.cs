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

using Jitendex.Warehouse.Jmdict;

namespace Jitendex.Warehouse;

public record JmdictPath(string XmlFile, string XRefCache);

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

        var jmdictPath = new JmdictPath
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

