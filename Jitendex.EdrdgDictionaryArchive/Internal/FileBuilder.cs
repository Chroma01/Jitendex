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
using Jitendex.MinimalPatch;

namespace Jitendex.EdrdgDictionaryArchive.Internal;

internal sealed class FileBuilder
{
    private readonly ILogger<FileBuilder> _logger;
    private readonly FileCache _cache;
    private readonly FileArchive _archive;

    public FileBuilder(ILogger<FileBuilder> logger, FileCache cache, FileArchive archive) =>
        (_logger, _cache, _archive) =
        (@logger, @cache, @archive);

    public FileInfo GetFile(DateOnly date)
    {
        if (date == default)
        {
            date = _archive.GetLatestPatchDate();
        }
        return _cache.GetExistingFile(date) ?? BuildFile(date);
    }

    public (FileInfo?, DateOnly) GetNextFile(DateOnly previousDate)
    {
        var date = _archive.GetNextPatchDate(previousDate);
        if (date == default)
        {
            return (null, date);
        }
        var file = _cache.GetExistingFile(date) ?? BuildFile(date);
        return (file, date);
    }

    private FileInfo BuildFile(DateOnly date)
    {
        var (baseDate, baseFile, patchDates) = GetBuildBase(date);

        int length = baseFile.Length();
        var @patchBuffer = (new char[length / 10]).AsSpan();
        var originBuffer = (new char[length * 3 / 2]).AsSpan();
        var outputBuffer = (new char[length * 3 / 2]).AsSpan();
        baseFile.ReadInto(originBuffer);

        foreach (var patchDate in patchDates)
        {
            _logger.LogInformation("Patching file to date {PatchDate:yyyy-MM-dd}", patchDate);
            var patchFile = _archive.GetPatchFile(patchDate);
            int patchLength = patchFile.ReadInto(patchBuffer);
            length = Patch.Apply
            (
                @patchBuffer[..patchLength],
                originBuffer[..length],
                outputBuffer
            );
            if (patchDate != date)
            {
                var originText = originBuffer[..length];
                var outputText = outputBuffer[..length];
                outputText.CopyTo(originText);
            }
        }

        var builtFile = _cache.SetFile(date, outputBuffer[..length]);
        _cache.DeleteFile(baseDate);
        return builtFile;
    }

    private (DateOnly, FileInfo, List<DateOnly>) GetBuildBase(DateOnly date)
    {
        DateOnly baseDate = default;
        FileInfo? baseFile = null;
        List<DateOnly> patchDates = [];
        foreach (var patchDate in _archive.GetPatchDates(untilDate: date))
        {
            if (_cache.GetExistingFile(patchDate) is FileInfo cachedFile)
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
