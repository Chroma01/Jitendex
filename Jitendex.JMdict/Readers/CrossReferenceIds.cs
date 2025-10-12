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
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Jitendex.JMdict.Readers;

internal partial class CrossReferenceIds
{
    private readonly ILogger<CrossReferenceIds> _logger;
    private readonly Files _files;
    private ReadOnlyDictionary<string, int>? _xrefToId = null;

    public CrossReferenceIds(ILogger<CrossReferenceIds> logger, Files files) =>
        (_logger, _files) =
        (@logger, @files);

    public async Task<ReadOnlyDictionary<string, int>> LoadAsync()
    {
        if (_files.XrefIds is null)
        {
            return new Dictionary<string, int>().AsReadOnly();
        }
        await using var stream = File.OpenRead(_files.XrefIds.FullName);
        var dictionary = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(stream) ?? [];
        _xrefToId = dictionary.AsReadOnly();
        return _xrefToId;
    }

    public async Task WriteAsync(Dictionary<string, object> dictionary)
    {
        if (_files.XrefIds is null)
        {
            return;
        }

        foreach (var xref in _xrefToId?.Keys.AsEnumerable() ?? [])
        {
            if (!dictionary.ContainsKey(xref))
            {
                LogUnusedKey(xref, _files.XrefIds.Name);
            }
        }

        await using var stream = File.OpenWrite
        (
            _files.XrefIds.FullName.EndsWith(".json")
                ? _files.XrefIds.FullName[..^5] + ".new.json"
                : _files.XrefIds.FullName + ".new"
        );

        await JsonSerializer.SerializeAsync(stream, dictionary, _options);
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        IndentSize = 4,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [LoggerMessage(LogLevel.Information,
    "Key `{key}` in file `{File}` was not used by the reference sequencer")]
    private partial void LogUnusedKey(string key, string file);
}
