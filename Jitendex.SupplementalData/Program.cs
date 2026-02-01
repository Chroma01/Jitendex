/*
Copyright (c) 2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.CommandLine;

namespace Jitendex.SupplementalData;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Option<DirectoryInfo> dataDirectoryOption = new("--data-directory")
        {
            Description = "Path to the jitendex-data directory",
        };

        Argument<ProgramArgument> programArgument = new("argument")
        {
            Description = "Operation to perform"
        };

        var rootCommand = new RootCommand("Jitendex.SupplementalData: Import or export Jitendex flat-file resources")
        {
            dataDirectoryOption,
            programArgument,
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

        var dataDir = parseResult.GetValue(dataDirectoryOption);

        switch (parseResult.GetRequiredValue(programArgument))
        {
            case ProgramArgument.Import:
                await Service.ImportAsync(dataDir);
                break;
            case ProgramArgument.Export:
                await Service.ExportAsync(dataDir);
                break;
        }

        return 0;
    }

    private enum ProgramArgument
    {
        Import,
        Export,
    }
}
