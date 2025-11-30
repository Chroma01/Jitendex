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

using Jitendex.AppDirectory;
using static Jitendex.AppDirectory.DataSubdirectory;
using static Jitendex.EdrdgDictionaryArchive.DictionaryFile;
using static Jitendex.EdrdgDictionaryArchive.Service;

namespace Jitendex.JMdict;

public static class Service
{
    public static async Task RunAsync(DateOnly date, DirectoryInfo? archiveDirectory, DirectoryInfo? jitendexDataDirectory)
    {
        var files = new Files
        {
            Jmdict = GetEdrdgFile(JMdict_e_examp, date, archiveDirectory),
            XrefIds = GetXrefFile(jitendexDataDirectory),
        };
        var reader = ReaderProvider.GetReader(files);
        var document = await reader.ReadAsync();
        await DatabaseInitializer.WriteAsync(document);
    }

    private static FileInfo? GetXrefFile(DirectoryInfo? jitendexDataDirectory)
        => (jitendexDataDirectory ?? DataHome.Get(JitendexData))
            .CreateSubdirectory("jmdict")
            .GetFiles("cross_reference_sequences.json") is var files and not []
                ? files.First()
                : null;
}
