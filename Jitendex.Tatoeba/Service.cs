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

using static Jitendex.EdrdgDictionaryArchive.DictionaryFile;
using static Jitendex.EdrdgDictionaryArchive.Service;
using Jitendex.Tatoeba.Database;
using Jitendex.Tatoeba.Models;
using Jitendex.Tatoeba.Readers;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace Jitendex.Tatoeba;

public static class Service
{
    public static async Task UpdateAsync(DateOnly previousDate, DirectoryInfo? archiveDirectory)
    {
        var (file, date) = GetNextEdrdgFile(examples, previousDate, archiveDirectory);
        var document = await ReadAsync(file, date);
        await DatabaseInitializer.WriteAsync(document);
    }

    private static async Task<Document> ReadAsync(FileInfo file, DateOnly date)
    {
        using var loggerFactory = CreateLoggerFactory();
        var logger = loggerFactory.CreateLogger<TatoebaReader>();

        await using FileStream fs = new(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using BrotliStream bs = new(fs, CompressionMode.Decompress);
        using StreamReader sr = new(bs);

        var reader = new TatoebaReader(logger, sr, date);
        return await reader.ReadAsync();
    }

    private static ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(static builder =>
            builder.AddSimpleConsole(static options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = false;
                options.TimestampFormat = "HH:mm:ss ";
            }));
}
