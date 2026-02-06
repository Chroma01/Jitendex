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

namespace Jitendex.EdrdgDictionaryArchive.Internal;

internal sealed class EdrdgArchiveService : IEdrdgArchiveService
{
    private readonly FileBuilder _builder;

    public EdrdgArchiveService(FileBuilder builder)
        => _builder = builder;

    public FileInfo? GetFile(DictionaryFile file, DateOnly date, DirectoryInfo? archiveDirectory = null)
        => _builder.GetFile(new(file, date, archiveDirectory));

    public (FileInfo, DateOnly)? GetNextFile(DictionaryFile file, DateOnly previousDate, DirectoryInfo? archiveDirectory = null)
        => _builder.GetNextFile(new(file, previousDate, archiveDirectory));

    public (FileInfo, DateOnly)? GetEarliestFile(DictionaryFile file, DirectoryInfo? archiveDirectory = null)
        => _builder.GetEarliestFile(new(file, default, archiveDirectory));

    public (FileInfo, DateOnly)? GetLatestFile(DictionaryFile file, DirectoryInfo? archiveDirectory = null)
        => _builder.GetLatestFile(new(file, default, archiveDirectory));
}
