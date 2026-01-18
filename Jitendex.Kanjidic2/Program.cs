/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Option<DateOnly> dateOption = new("--date")
        {
            Description = "Date of the kanjidic2.xml file to retrieve"
        };

        Option<DirectoryInfo> archiveDirOption = new("--archive-path")
        {
            Description = "Path to the edrdg-dictionary-archive directory",
        };

        var rootCommand = new RootCommand("Jitendex.Kanjidic2: Import a Kanjidic2 XML document")
        {
            dateOption,
            archiveDirOption,
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

        await Service.RunAsync
        (
            date: parseResult.GetValue(dateOption),
            archiveDirectory: parseResult.GetValue(archiveDirOption)
        );

        return 0;
    }
}
