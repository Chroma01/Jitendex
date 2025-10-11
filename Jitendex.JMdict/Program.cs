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
using System.CommandLine;
using Jitendex.JMdict.Models;

namespace Jitendex.JMdict;

public class Program
{
    public static async Task Main(string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();

        var jmdictFileArgument = new Argument<FileInfo>("jmdict-file")
        {
            Description = "Path to JMdict XML file",
        };

        var xrefIdsFileArgument = new Argument<FileInfo>("xref-ids")
        {
            Description = "Path to JSON file containing cross-reference keys and corresponding entry ID values",
        };

        var rootCommand = new RootCommand("Jitendex.JMdict: Import a JMdict XML document")
        {
            jmdictFileArgument,
            xrefIdsFileArgument,
        };

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            foreach (var parseError in parseResult.Errors)
            {
                Console.Error.WriteLine(parseError.Message);
            }
            return;
        }

        var jmdictFile = parseResult.GetValue(jmdictFileArgument)!;
        var xrefIdsFile = parseResult.GetValue(xrefIdsFileArgument)!;

        var jmdict = await GetJmdictAsync(jmdictFile, xrefIdsFile);
        await DatabaseInitializer.WriteToNewDatabase(jmdict);

        Console.WriteLine($"Finished in {double.Round(sw.Elapsed.TotalSeconds, 1)} seconds.");
    }

    public static async Task<JmdictDocument> GetJmdictAsync(FileInfo jmdictFile, FileInfo xrefIdsFile)
    {
        var jmdictPaths = new FilePaths
        {
            Jmdict = jmdictFile.FullName,
            XrefIds = xrefIdsFile.FullName,
        };

        var reader = await JmdictReaderProvider.GetReaderAsync(jmdictPaths);
        var jmdict = await reader.ReadJmdictAsync();
        return jmdict;
    }
}
