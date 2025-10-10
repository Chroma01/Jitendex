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

using System.Formats.Tar;
using System.IO.Compression;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace Jitendex.KanjiVG.Readers;

internal class KanjiFiles
{
    private readonly ILogger<KanjiFiles> _logger;
    private readonly FilePaths _filePaths;
    private readonly XmlReaderSettings _xmlReaderSettings = new()
    {
        Async = true,
        DtdProcessing = DtdProcessing.Parse,
        MaxCharactersFromEntities = long.MaxValue,
        MaxCharactersInDocument = long.MaxValue,
    };

    public KanjiFiles(ILogger<KanjiFiles> logger, FilePaths filePaths) =>
        (_logger, _filePaths) =
        (@logger, @filePaths);

    public async IAsyncEnumerable<(string Name, XmlReader Reader)> EnumerateAsync()
    {
        await using FileStream fs = new(_filePaths.SvgArchive, FileMode.Open, FileAccess.Read);
        await using BrotliStream br = new(fs, CompressionMode.Decompress);
        await using TarReader tarReader = new(br);

        while (await tarReader.GetNextEntryAsync() is TarEntry entry)
        {
            if (entry.DataStream is null)
            {
                _logger.LogWarning("Data stream for file {Name} is empty", entry.Name);
                continue;
            }
            using var xmlReader = XmlReader.Create(entry.DataStream, _xmlReaderSettings);
            yield return (entry.Name, xmlReader);
        }
    }
}
