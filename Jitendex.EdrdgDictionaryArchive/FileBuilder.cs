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
        if (date.Equals(default))
        {
            date = _archive.GetLatestPatchDate();
        }
        if (_cache.GetFile(date) is FileInfo cachedFile)
        {
            return cachedFile;
        }

        var (baseDate, baseFile) = GetBaseFiles(date);
        var patchDates = _archive.EnumeratePatchDates(afterDate: baseDate, untilDate: date);

        Console.Error.WriteLine($"Reading base file from date {baseDate}");

        int arraySize = int.MaxValue / 10;
        var patchText = new char[arraySize];
        var originalText = new char[arraySize];
        var newText = new char[arraySize];
        var textLength = baseFile.ReadInto(originalText);

        foreach (var patchDate in patchDates)
        {
            Console.Error.WriteLine($"Patching file to date {patchDate}");
            var patchFile = _archive.GetPatchFile(patchDate);
            int patchLength = patchFile.ReadInto(patchText);
            textLength = Patch.Apply
            (
                patchText.AsSpan(0, patchLength),
                originalText.AsSpan(0, textLength),
                newText
            );
            newText.AsSpan(0, textLength)
                .CopyTo(originalText.AsSpan(0, textLength));  // Not necessary on the final loop, but it's no big deal.
        }

        var file = _cache.SetFile(date, newText.AsSpan(0, textLength));

        return file;
    }

    private (DateOnly, FileInfo) GetBaseFiles(DateOnly date)
    {
        DateOnly baseDate = default;
        FileInfo? baseFile = null;
        foreach (var patchDate in _archive.EnumeratePatchDates(afterDate: default))
        {
            if (patchDate == date)
            {
                break;
            }
            if (_cache.GetFile(patchDate) is FileInfo cachedFile)
            {
                baseDate = patchDate;
                baseFile = cachedFile;
            }
        }
        baseFile ??= _archive.BaseFile;
        return (baseDate, baseFile);
    }
}

internal static class Patch
{
    public static int Apply(ReadOnlySpan<char> patchText, ReadOnlySpan<char> originalText, Span<char> newText)
    {
        throw new NotImplementedException();
    }
}
