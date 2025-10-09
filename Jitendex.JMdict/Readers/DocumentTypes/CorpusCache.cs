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
using Jitendex.JMdict.Models;

namespace Jitendex.JMdict.Readers.DocumentTypes;

internal partial class CorpusCache(ILogger<CorpusCache> logger)
{
    public IEnumerable<Corpus> Corpora { get => _cache.Values; }

    private readonly Dictionary<CorpusId, Corpus> _cache = [];

    public Corpus GetCorpus(Entry entry)
    {
        var id = EntryIdToCorpusId(entry.Id);

        if (id == CorpusId.Unknown)
        {
            LogUnknownCorpusEntry(entry.Id);
        }

        if (_cache.TryGetValue(id, out Corpus? corpus))
        {
            corpus.Entries.Add(entry);
            return corpus;
        }

        var newCorpus = new Corpus
        {
            Id = id,
            Name = id.ToString(),
            Entries = [entry],
        };

        _cache.Add(id, newCorpus);
        return newCorpus;
    }

    private static CorpusId EntryIdToCorpusId(int entryId) => entryId switch
    {
        (>= 1000000) and (< 3000000) => CorpusId.Jmdict,
        (>= 5000000) and (< 6000000) => CorpusId.Jmnedict,
                             9999999 => CorpusId.Metadata,
                                   _ => CorpusId.Unknown,
    };

    [LoggerMessage(LogLevel.Warning,
    "Entry ID `{EntryId}` belongs to an unknown corpus.")]
    private partial void LogUnknownCorpusEntry(int entryId);
}
