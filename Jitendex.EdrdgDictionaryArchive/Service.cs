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
using Jitendex.EdrdgDictionaryArchive.Internal;

namespace Jitendex.EdrdgDictionaryArchive;

public static class Service
{
    public static FileInfo GetEdrdgFile(DictionaryFile file, DateOnly date, DirectoryInfo? archiveDirectory = null)
    {
        using var loggerFactory = CreateLoggerFactory();
        var logger = loggerFactory.CreateLogger<FileBuilder>();
        FileType type = new(file);
        FileBuilder builder = new
        (
            logger,
            new FileCache(type),
            new FileArchive(type, archiveDirectory)
        );
        return builder.GetFile(date);
    }

    public static (FileInfo?, DateOnly) GetNextEdrdgFile(DictionaryFile file, DateOnly previousDate, DirectoryInfo? archiveDirectory = null)
    {
        using var loggerFactory = CreateLoggerFactory();
        var logger = loggerFactory.CreateLogger<FileBuilder>();
        FileType type = new(file);
        FileBuilder builder = new
        (
            logger,
            new FileCache(type),
            new FileArchive(type, archiveDirectory)
        );
        return builder.GetNextFile(previousDate);
    }

    public static List<DateOnly> GetEdrdgFileDates(DictionaryFile file, DateOnly afterDate, DirectoryInfo? archiveDirectory = null)
    {
        FileType type = new(file);
        FileArchive archive = new(type, archiveDirectory);
        return archive.GetPatchDates(afterDate: afterDate);
    }

    private static ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(static builder =>
            builder.AddSimpleConsole(static options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }));
}

public enum DictionaryFile : byte
{
    JMdict,
    JMdict_e,
    JMdict_e_examp,
    JMnedict,
    kanjidic2,
    examples,
}
