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
using Jitendex.Import.Jmdict.Models;

namespace Jitendex.Import.Jmdict.Readers;

internal partial class CorpusCache(ILogger<CorpusCache> logger)
{
    private readonly Dictionary<CorpusId, Corpus> _cache = [];

    public Corpus GetCorpus(int entryId)
    {
        var id = EntryIdToCorpusId(entryId);

        if (id == CorpusId.Unknown)
        {
            LogUnknownCorpusEntry(entryId);
        }

        if (_cache.TryGetValue(id, out Corpus? corpus))
        {
            return corpus;
        }

        var newCorpus = new Corpus
        {
            Id = id,
            Name = id.ToString(),
        };

        _cache.Add(id, newCorpus);
        return newCorpus;
    }

    private static CorpusId EntryIdToCorpusId(int entryId) =>
        entryId switch
        {
            < 1000000 => CorpusId.Unknown,
            < 3000000 => CorpusId.Jmdict,
            < 5000000 => CorpusId.Unknown,
            < 6000000 => CorpusId.Jmnedict,
            < 9999999 => CorpusId.Unknown,
              9999999 => CorpusId.Metadata,
                    _ => CorpusId.Unknown,
        };

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{EntryId}` belongs to an unknown corpus.")]
    private partial void LogUnknownCorpusEntry(int entryId);
}
