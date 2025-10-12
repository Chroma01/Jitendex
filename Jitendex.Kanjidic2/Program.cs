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

using System.CommandLine;

namespace Jitendex.Kanjidic2;

public class Program
{
    public static async Task Main(string[] args)
    {
        var kanjidic2FileArgument = new Argument<FileInfo>("kanjidic2-file")
        {
            Description = "Path to Brotli-compressed Kanjidic2 XML file",
        };

        var rootCommand = new RootCommand("Jitendex.Kanjidic2: Import a Kanjidic2 XML document")
        {
            kanjidic2FileArgument
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

        var files = new Files
        {
            Kanjidic2 = parseResult.GetRequiredValue(kanjidic2FileArgument)
        };

        var reader = ReaderProvider.GetReader(files);
        var kanjidic2 = await reader.ReadAsync();

        await DatabaseInitializer.WriteAsync(kanjidic2);
    }
}
