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

namespace Jitendex.EdrdgDictionaryArchive;

public class Program
{
    public static int Main(string[] args)
    {
        Option<DirectoryInfo> archiveDirOption = new("--archive-path")
        {
            Description = "Path to the edrdg-dictionary-archive directory",
        };

        Option<DateOnly> dateOption = new("--date")
        {
            Description = "Date of the file to retrieve"
        };

        Argument<string> filenameArgument = new("filename");
        filenameArgument.Validators.Add(result =>
        {
            try
            {
                _ = new FileType(result.GetValue(filenameArgument));
            }
            catch(Exception ex)
            {
                result.AddError(ex.Message);
            }
        });

        var rootCommand = new RootCommand("Jitendex.EdrdgDictionaryArchive: Retrieve a versioned file from the EDRDG Dictionary Archive")
        {
            archiveDirOption,
            dateOption,
            filenameArgument,
        };

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            foreach (var parseError in parseResult.Errors)
            {
                Console.Error.WriteLine(parseError.Message);
            }
            return 1;
        }

        var type = new FileType(parseResult.GetRequiredValue(filenameArgument));
        var archiveDir = parseResult.GetValue(archiveDirOption);
        var date = parseResult.GetValue(dateOption);

        FileCache cache = new(type);
        FileArchive archive = new(archiveDir, type);
        FileBuilder builder = new(cache, archive);

        var file = builder.GetFile(date);

        Console.WriteLine(file.FullName);
        return 0;
    }
}
