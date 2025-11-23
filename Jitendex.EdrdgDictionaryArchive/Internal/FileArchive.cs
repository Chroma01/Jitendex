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

using System.Collections.ObjectModel;

namespace Jitendex.EdrdgDictionaryArchive.Internal;

internal sealed class FileArchive
{
    private readonly DirectoryInfo _patchesDirectory;
    public FileInfo BaseFile { get; }

    public FileArchive(FileType type, DirectoryInfo? directoryInfo)
    {
        var root = directoryInfo ?? GetDefaultDirectory();
        _patchesDirectory = GetPatchesDirectory(root, type);
        BaseFile = GetBaseFile(root, type);
    }

    private static DirectoryInfo GetDefaultDirectory()
    {
        var path = Path.Join
        (
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "jitendex",
            "edrdg-dictionary-archive"
        );
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Path to archive directory not specified. No archive found at the default path '{path}'");
        }
        return new DirectoryInfo(path);
    }

    private static DirectoryInfo GetPatchesDirectory(DirectoryInfo root, FileType type) => new
    (
        Path.Join
        (
            root.FullName,
            type.DirectoryName,
            "patches"
        )
    );

    private static FileInfo GetBaseFile(DirectoryInfo root, FileType type) => new
    (
        Path.Join
        (
            root.FullName,
            type.DirectoryName,
            type.CompressedName
        )
    );

    public DateOnly GetLatestPatchDate()
    {
        var yearDir = _patchesDirectory.GetSortedDirectories().Last();
        var monthDir = yearDir.GetSortedDirectories().Last();
        var patchFile = monthDir.GetSortedFiles().Last();
        return new DateOnly
        (
            year: int.Parse(yearDir.Name),
            month: int.Parse(monthDir.Name),
            day: int.Parse(patchFile.Name.AsSpan(0, 2))
        );
    }

    public ReadOnlyCollection<DateOnly> GetPatchDates(DateOnly untilDate = default)
    {
        List<DateOnly> patchDates = [];
        foreach (var yearDir in _patchesDirectory.GetSortedDirectories())
        {
            foreach (var monthDir in yearDir.GetSortedDirectories())
            {
                foreach (var patchFile in monthDir.GetSortedFiles())
                {
                    var patchDate = new DateOnly
                    (
                        year: int.Parse(yearDir.Name),
                        month: int.Parse(monthDir.Name),
                        day: int.Parse(patchFile.Name.AsSpan(0, 2))
                    );
                    patchDates.Add(patchDate);
                    if (patchDate == untilDate)
                    {
                        goto end;
                    }
                }
            }
        }
        if (untilDate != default)
        {
            throw new ArgumentException($"Patch for date {untilDate} does not exist in the archive", nameof(untilDate));
        }
    end:
        return patchDates.AsReadOnly();
    }

    public FileInfo GetPatchFile(DateOnly date) => new
    (
        Path.Join
        (
            _patchesDirectory.FullName,
            date.Year.ToString(),
            $"{date.Month:D2}",
            $"{date.Day:D2}.patch.br"
        )
    );
}
