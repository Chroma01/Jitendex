/*
Copyright (c) 2025, 2026 Stephen Kraus
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

using Jitendex.AppDirectory;
using static Jitendex.AppDirectory.DataSubdirectory;

namespace Jitendex.EdrdgDictionaryArchive.Internal;

internal sealed class FileArchive
{
    private readonly DirectoryInfo _patchesDirectory;
    public FileInfo BaseFile { get; }

    public FileArchive(FileType type, DirectoryInfo? directoryInfo)
    {
        var dataRoot = directoryInfo ?? DataHome.Get(EdrdgArchiveDirectory);
        _patchesDirectory = GetPatchesDirectory(dataRoot, type);
        BaseFile = GetBaseFile(dataRoot, type);
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

    public DateOnly? GetNextPatchDate(DateOnly previousDate)
    {
        foreach (var yearDir in _patchesDirectory.GetSortedDirectories())
        {
            int year = int.Parse(yearDir.Name);
            if (year < previousDate.Year)
            {
                continue;
            }
            foreach (var monthDir in yearDir.GetSortedDirectories())
            {
                int month = int.Parse(monthDir.Name);
                if (year == previousDate.Year && month < previousDate.Month)
                {
                    continue;
                }
                foreach (var patchFile in monthDir.GetSortedFiles())
                {
                    int day = int.Parse(patchFile.Name.AsSpan(0, 2));
                    var date = new DateOnly(year, month, day);
                    if (previousDate < date)
                    {
                        return date;
                    }
                }
            }
        }
        return null;
    }

    public List<DateOnly> GetPatchDates(DateOnly afterDate = default, DateOnly untilDate = default)
    {
        List<DateOnly> patchDates = [];
        foreach (var yearDir in _patchesDirectory.GetSortedDirectories())
        {
            int year = int.Parse(yearDir.Name);
            if (year < afterDate.Year)
            {
                continue;
            }
            foreach (var monthDir in yearDir.GetSortedDirectories())
            {
                int month = int.Parse(monthDir.Name);
                if (month < afterDate.Month)
                {
                    continue;
                }
                foreach (var patchFile in monthDir.GetSortedFiles())
                {
                    int day = int.Parse(patchFile.Name.AsSpan(0, 2));
                    DateOnly patchDate = new(year, month, day);
                    if (untilDate != default && patchDate > untilDate)
                    {
                        goto end;
                    }
                    if (patchDate > afterDate)
                    {
                        patchDates.Add(patchDate);
                    }
                }
            }
        }
    end:
        return patchDates;
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
