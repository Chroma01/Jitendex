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

namespace Jitendex.Warehouse.Jmdict.Models;

public enum CorpusId
{
    Unknown,
    Jmdict,
    Jmnedict,
    Metadata,
}

public class Corpus : IKeyword
{
    public CorpusId Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    internal static CorpusId EntryIdToCorpusId(int entryId)
    {
        var corpusId = entryId switch
        {
            < 1000000 => CorpusId.Unknown,
            < 3000000 => CorpusId.Jmdict,
            < 5000000 => CorpusId.Unknown,
            < 6000000 => CorpusId.Jmnedict,
            < 9999999 => CorpusId.Unknown,
              9999999 => CorpusId.Metadata,
                    _ => CorpusId.Unknown,
        };
        // TODO: Log warning if unknown.
        return corpusId;
    }

    internal static readonly Dictionary<CorpusId, string> IdToDescription = new()
    {
        [CorpusId.Unknown] = "Error",
        [CorpusId.Jmdict] = "Japanese Multilingual Dictionary",
        [CorpusId.Jmnedict] = "Japanese Multilingual Named Entity Dictionary",
        [CorpusId.Metadata] = "Metadata added in post-processing",
    };
}
