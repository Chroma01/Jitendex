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

using System.IO.Compression;

namespace Jitendex.EdrdgDictionaryArchive;

internal sealed class FileCache
{
    private readonly DirectoryInfo _directory;

    public FileCache(FileType type)
    {
        var path = Path.Join(GetCacheRoot(), "jitendex", "edrdg-dictionary-archive", type.DirectoryName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        _directory = new DirectoryInfo(path);
    }

    private static ReadOnlySpan<char> GetCacheRoot() => Environment.OSVersion.Platform switch
    {
        PlatformID.Unix
            => Environment.GetEnvironmentVariable("XDG_CACHE_HOME")
            ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache"),
        _
            => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
    };

    public FileInfo? GetFile(DateOnly date)
        => GetCachedFilePath(date) is var cachedFile
        && File.Exists(cachedFile)
            ? new FileInfo(cachedFile)
            : null;

    public FileInfo SetFile(DateOnly date, ReadOnlySpan<char> text)
    {
        var cachedFile = GetCachedFilePath(date);
        using (FileStream f = new(cachedFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            using BrotliStream b = new(f, CompressionLevel.Optimal);
            using StreamWriter s = new(b);
            s.Write(text);
        }
        return new FileInfo(cachedFile);
    }

    private string GetCachedFilePath(DateOnly date) => Path.Join
    (
        _directory.FullName,
        $"{date.Year}-{date.Month:D2}-{date.Day:D2}.br"
    );
}
