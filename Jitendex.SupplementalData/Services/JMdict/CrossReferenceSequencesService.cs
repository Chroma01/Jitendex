/*
Copyright (c) 2026 Stephen Kraus
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

using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Jitendex.AppDirectory;

namespace Jitendex.SupplementalData.Services.JMdict;

internal sealed class CrossReferenceSequencesService
{
    private readonly SupplementContext _context;

    public CrossReferenceSequencesService(SupplementContext context)
        => _context = @context;

    public async Task ExportAsync(DirectoryInfo? dataDir)
    {
        var dictionary = _context.CrossReferenceSequences.ToDictionary(
            keySelector: static x => x.ToExportKey(),
            elementSelector: static x => x.RefSequenceId);

        await WriteJsonDictionaryAsync(dataDir, dictionary);
    }

    public async Task ImportAsync(DirectoryInfo? dataDir)
    {
        var dictionary = await ReadJsonDictionaryAsync(dataDir);

        using var transaction = _context.Database.BeginTransaction();
        _context.CrossReferenceSequences.ExecuteDelete();

        foreach (var (key, value) in dictionary)
        {
            var parsedKey = CrossReferenceSequenceKeyParser.ParseKey(key);
            _context.CrossReferenceSequences.Add(new()
            {
                SequenceId = parsedKey.SequenceId,
                SenseNumber = parsedKey.SenseNumber,
                RefText1 = parsedKey.RefText1,
                RefText2 = parsedKey.RefText2 ?? string.Empty,
                RefSenseNumber = parsedKey.RefSenseNumber,
                RefSequenceId = value,
            });
        }

        _context.SaveChanges();
        transaction.Commit();
    }

    private async Task<Dictionary<string, int?>> ReadJsonDictionaryAsync(DirectoryInfo? dataDir)
    {
        var filePath = GetJsonFilePath(dataDir);
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<Dictionary<string, int?>>(stream) ?? [];
    }

    private async Task WriteJsonDictionaryAsync(DirectoryInfo? dataDir, Dictionary<string, int?> dictionary)
    {
        var filePath = GetJsonFilePath(dataDir);
        await using var stream = File.OpenWrite(filePath);
        await JsonSerializer.SerializeAsync(stream, dictionary, GetJsonSerializerOptions());
    }

    private string GetJsonFilePath(DirectoryInfo? dataDir)
    {
        dataDir ??= DataHome.Get(DataSubdirectory.JitendexDataDirectory);
        return Path.Join(dataDir.FullName, "jmdict", "cross_reference_sequences.json");
    }

    private static JsonSerializerOptions GetJsonSerializerOptions() => new()
    {
        WriteIndented = true,
        IndentSize = 4,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}

internal static class CrossReferenceSequenceKeyParser
{
    public sealed record ParsedKey(
        int SequenceId,
        int SenseNumber,
        string RefText1,
        string? RefText2,
        int RefSenseNumber);

    public static ParsedKey ParseKey(string key)
    {
        var splitKey = key.Split('・');
        var sequenceId = int.Parse(splitKey[0]);
        var senseNumber = int.Parse(splitKey[1]);
        var (refText1, refText2) = ParseText(splitKey[2]);
        var refSenseNumber = int.Parse(splitKey[3]);
        return new ParsedKey(sequenceId, senseNumber, refText1, refText2, refSenseNumber);
    }

    private static (string, string?) ParseText(string text)
    {
        string refText1;
        string? refText2 = null;
        if (text.EndsWith('】') && text.IndexOf('【') is int index and not -1)
        {
            refText1 = text[..index];
            refText2 = text[(index + 1)..^1];
        }
        else
        {
            refText1 = text;
        }
        return (refText1, refText2);
    }
}
