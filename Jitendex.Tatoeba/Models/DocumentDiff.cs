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

using Microsoft.AspNetCore.JsonPatch.SystemTextJson;

namespace Jitendex.Tatoeba.Models;

internal sealed class DocumentDiff
{
    public DateOnly Date { get; init; }
    public Dictionary<int, JsonPatchDocument<Sequence>> ABPatches { get; init; }
    public Dictionary<int, JsonPatchDocument<Sequence>> BAPatches { get; init; }

    public DocumentDiff(Document docA, Document docB)
    {
        Date = docB.Metadata.Date;
        ABPatches = [];
        BAPatches = [];

        foreach (var (key, valueA) in docA.Sequences)
        {
            if (!docB.Sequences.TryGetValue(key, out var valueB))
            {
                var emptyB = new Sequence
                {
                    Id = key,
                    CreatedDate = docB.Metadata.Date,
                };
                ABPatches[key] = SequenceDiff(valueA, emptyB);
                BAPatches[key] = SequenceDiff(emptyB, valueA);
            }
            else if (valueA != valueB)
            {
                ABPatches[key] = SequenceDiff(valueA, valueB);
                BAPatches[key] = SequenceDiff(valueB, valueA);
            }
        }
        foreach (var (key, valueB) in docB.Sequences)
        {
            if (!docA.Sequences.ContainsKey(key))
            {
                var emptyA = new Sequence
                {
                    Id = key,
                    CreatedDate = docA.Metadata.Date,
                };
                ABPatches[key] = SequenceDiff(emptyA, valueB);
                BAPatches[key] = SequenceDiff(valueB, emptyA);
            }
        }
    }

    private static JsonPatchDocument<Sequence> SequenceDiff(Sequence a, Sequence b)
    {
        var diff = new JsonPatchDocument<Sequence>();

        if (a.EnglishSentence != b.EnglishSentence)
        {
            diff.Test(path: static x => x.EnglishSentence, value: a.EnglishSentence);
            diff.Replace(path: static x => x.EnglishSentence, value: b.EnglishSentence);
        }

        if (a.JapaneseSentence != b.JapaneseSentence)
        {
            diff.Test(path: static x => x.JapaneseSentence, value: a.JapaneseSentence);
            diff.Replace(path: static x => x.JapaneseSentence, value: b.JapaneseSentence);
        }

        return diff;
    }
}
