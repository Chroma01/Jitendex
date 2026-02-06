/*
Copyright (c) 2025-2026 Stephen Kraus
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

    public FileInfo GetFile(FileRequest request)
        => _cache.GetExistingFile(request) ?? BuildFile(request);

    public (FileInfo?, DateOnly) GetNextFile(FileRequest request)
    {
        if (_archive.GetNextPatchDate(request) is not DateOnly nextDate)
        {
            return (null, default);
        }
        var nextFileRequest = request with { Date = nextDate };
        var nextFile = GetFile(nextFileRequest);
        return (nextFile, nextDate);
    }

    public (FileInfo?, DateOnly) GetLatestFile(FileRequest request)
    {
        var latestDate = _archive.GetLatestPatchDate(request);
        var latestFileRequest = request with { Date = latestDate };
        var latestFile = GetFile(latestFileRequest);
        return (latestFile, latestDate);
    }

    private FileInfo BuildFile(FileRequest request)
    {
        var (baseDate, baseFile, patches) = GetBuildBase(request);

        int length = baseFile.Length();
        Span<char> @patchBuffer = new char[length / 10];
        Span<char> originBuffer = new char[length * 3 / 2];
        Span<char> outputBuffer = new char[length * 3 / 2];
        baseFile.ReadInto(originBuffer);

        foreach (var patch in patches)
        {
            _logger.LogInformation("Patching file to date {Date:yyyy-MM-dd}", patch.Date);
            var patchFile = new FileInfo(patch.Path);
            int patchLength = patchFile.ReadInto(patchBuffer);

            length = Patcher.ApplyPatch
            (
                @patchBuffer[..patchLength],
                originBuffer[..length],
                outputBuffer
            );

            outputBuffer[..length].CopyTo(originBuffer[..length]);
        }

        var builtFile = _cache.WriteFile(request, outputBuffer[..length]);

        var baseFileRequest = request with { Date = baseDate };
        _cache.DeleteFile(baseFileRequest);

        return builtFile;
    }

    private (DateOnly, FileInfo, List<Patch>) GetBuildBase(FileRequest request)
    {
        DateOnly baseDate = default;
        FileInfo? baseFile = null;
        List<Patch> patches = [];

        foreach (var patch in _archive.GetPatches(request))
        {
            var cachedFileRequest = request with { Date = patch.Date };
            if (_cache.GetExistingFile(cachedFileRequest) is FileInfo cachedFile)
            {
                baseDate = patch.Date;
                baseFile = cachedFile;
                patches.Clear();
            }
            else
            {
                patches.Add(patch);
            }
        }

        baseFile ??= _archive.BaseFile(request);

        return (baseDate, baseFile, patches);
    }
}
