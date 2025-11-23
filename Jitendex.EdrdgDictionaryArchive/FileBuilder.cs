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

namespace Jitendex.EdrdgDictionaryArchive;

internal sealed class FileBuilder
{
    private readonly FileCache _cache;
    private readonly FileArchive _archive;

    public FileBuilder(FileCache cache, FileArchive archive)
        => (_cache, _archive) = (cache, archive);

    public FileInfo GetFile(DateOnly date)
    {
        if (date == default)
        {
            date = _archive.GetLatestPatchDate();
        }
        return _cache.GetFile(date) is FileInfo cachedFile
            ? cachedFile
            : BuildFile(date);
    }

    private FileInfo BuildFile(DateOnly date)
    {
        var (baseDate, baseFile, patchDates) = GetBuildBase(date);

        Console.Error.WriteLine($"Reading base file from date {baseDate}");

        int length = baseFile.Length();
        var patchText = new char[length / 10];
        var originalText = new char[length * 3 / 2];
        var newText = new char[length * 3 / 2];
        baseFile.ReadInto(originalText);

        foreach (var patchDate in patchDates)
        {
            Console.Error.WriteLine($"Patching file to date {patchDate}");
            var patchFile = _archive.GetPatchFile(patchDate);
            int patchLength = patchFile.ReadInto(patchText);
            length = Patch.Apply
            (
                patchText.AsSpan(0, patchLength),
                originalText.AsSpan(0, length),
                newText
            );

            newText.AsSpan(0, length)
                .CopyTo(originalText.AsSpan(0, length));  // Not necessary on the final loop, but it's no big deal.
        }

        return _cache.SetFile(date, newText.AsSpan(0, length));
    }

    private (DateOnly, FileInfo, List<DateOnly>) GetBuildBase(DateOnly date)
    {
        DateOnly baseDate = default;
        FileInfo? baseFile = null;
        List<DateOnly> patchDates = [];
        foreach (var patchDate in _archive.GetPatchDates(untilDate: date))
        {
            if (_cache.GetFile(patchDate) is FileInfo cachedFile)
            {
                baseDate = patchDate;
                baseFile = cachedFile;
                patchDates.Clear();
            }
            else
            {
                patchDates.Add(patchDate);
            }
        }
        baseFile ??= _archive.BaseFile;
        return (baseDate, baseFile, patchDates);
    }
}

internal static class Patch
{
    public static int Apply(ReadOnlySpan<char> patchText, ReadOnlySpan<char> originalText, Span<char> newText)
    {
        throw new NotImplementedException();
    }
}
