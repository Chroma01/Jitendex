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
using Microsoft.AspNetCore.JsonPatch.SystemTextJson.Operations;

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
            else if (!valueA.Equals(valueB))
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

    private JsonPatchDocument<Sequence> SequenceDiff(Sequence a, Sequence b)
    {
        var diff = new JsonPatchDocument<Sequence>();

        if (a.EnglishSentence is not null && b.EnglishSentence is not null)
        {
            diff.Operations.AddRange(EnglishSentenceDiff(a.EnglishSentence, b.EnglishSentence));
        }
        else if (a.EnglishSentence is not null || b.EnglishSentence is not null)
        {
            diff.Test(path: static x => x.EnglishSentence, value: a.EnglishSentence);
            diff.Replace(path: static x => x.EnglishSentence, value: b.EnglishSentence);
        }

        if (a.JapaneseSentence is not null && b.JapaneseSentence is not null)
        {
            diff.Operations.AddRange(JapaneseSentenceDiff(a.JapaneseSentence, b.JapaneseSentence));
        }
        else if (a.JapaneseSentence is not null || b.JapaneseSentence is not null)
        {
            diff.Test(path: static x => x.JapaneseSentence, value: a.JapaneseSentence);
            diff.Replace(path: static x => x.JapaneseSentence, value: b.JapaneseSentence);
        }

        return diff;
    }

    private static List<Operation<Sequence>> EnglishSentenceDiff(EnglishSentence a, EnglishSentence b)
    {
        var diff = new JsonPatchDocument<Sequence>();

        if (!string.Equals(a.Text, b.Text, StringComparison.Ordinal))
        {
            diff.Test(path: static x => x.EnglishSentence!.Text, value: a.Text);
            diff.Replace(path: static x => x.EnglishSentence!.Text, value: b.Text);
        }

        return diff.Operations;
    }

    private static List<Operation<Sequence>> JapaneseSentenceDiff(JapaneseSentence a, JapaneseSentence b)
    {
        var diff = new JsonPatchDocument<Sequence>();

        if (!string.Equals(a.Text, b.Text, StringComparison.Ordinal))
        {
            diff.Test(path: static x => x.JapaneseSentence!.Text, value: a.Text);
            diff.Replace(path: static x => x.JapaneseSentence!.Text, value: b.Text);
        }

        if (!Enumerable.SequenceEqual(a.Indices, b.Indices))
        {
            if (a.Indices.Count <= b.Indices.Count)
            {
                for (int i = 0; i < b.Indices.Count; i++)
                {
                    var x = i < a.Indices.Count ? a.Indices[i] : null;
                    var y = b.Indices[i];
                    diff.Operations.AddRange(IndexDiff(x, y, i));
                }
            }
            else
            {
                for (int i = a.Indices.Count - 1; i >= 0; i--)
                {
                    var x = a.Indices[i];
                    var y = i < b.Indices.Count ? b.Indices[i] : null;
                    diff.Operations.AddRange(IndexDiff(x, y, i));
                }
            }
        }

        return diff.Operations;
    }

    private static List<Operation<Sequence>> IndexDiff(SentenceIndex? a, SentenceIndex? b, int i)
    {
        var diff = new JsonPatchDocument<Sequence>();

        if (a is not null && b is not null)
        {
            if (a.MeaningId != b.MeaningId)
            {
                diff.Test(path: x => x.JapaneseSentence!.Indices[i].MeaningId, value: a.MeaningId);
                diff.Replace(path: x => x.JapaneseSentence!.Indices[i].MeaningId, value: b.MeaningId);
            }

            if (!Enumerable.SequenceEqual(a.Elements, b.Elements))
            {
                if (a.Elements.Count <= b.Elements.Count)
                {
                    for (int j = 0; j < b.Elements.Count; j++)
                    {
                        var x = j < a.Elements.Count ? a.Elements[j] : null;
                        var y = b.Elements[j];
                        diff.Operations.AddRange(ElementDiff(x, y, i, j));
                    }
                }
                else
                {
                    for (int j = a.Elements.Count - 1; j >= 0; j--)
                    {
                        var x = a.Elements[j];
                        var y = j < b.Elements.Count ? b.Elements[j] : null;
                        diff.Operations.AddRange(ElementDiff(x, y, i, j));
                    }
                }
            }
        }
        else if (a is not null)
        {
            diff.Test(path: static x => x.JapaneseSentence!.Indices!, value: a, position: i);
            diff.Remove(path: static x => x.JapaneseSentence!.Indices!, position: i);
        }
        else if (b is not null)
        {
            diff.Add(path: static x => x.JapaneseSentence!.Indices!, value: b, position: i);
        }

        return diff.Operations;
    }

    private static List<Operation<Sequence>> ElementDiff(IndexElement? a, IndexElement? b, int i, int j)
    {
        var diff = new JsonPatchDocument<Sequence>();

        if (a is not null && b is not null)
        {
            if (!string.Equals(a.Headword, b.Headword, StringComparison.Ordinal))
            {
                diff.Test(path: x => x.JapaneseSentence!.Indices[i].Elements[j].Headword, value: a.Headword);
                diff.Replace(path: x => x.JapaneseSentence!.Indices[i].Elements[j].Headword, value: b.Headword);
            }

            if (!string.Equals(a.Reading, b.Reading, StringComparison.Ordinal))
            {
                diff.Test(path: x => x.JapaneseSentence!.Indices[i].Elements[j].Reading, value: a.Reading);
                diff.Replace(path: x => x.JapaneseSentence!.Indices[i].Elements[j].Reading, value: b.Reading);
            }

            if (a.EntryId != b.EntryId)
            {
                diff.Test(path: x => x.JapaneseSentence!.Indices[i].Elements[j].EntryId, value: a.EntryId);
                diff.Replace(path: x => x.JapaneseSentence!.Indices[i].Elements[j].EntryId, value: b.EntryId);
            }

            if (a.SenseNumber != b.SenseNumber)
            {
                diff.Test(path: x => x.JapaneseSentence!.Indices[i].Elements[j].SenseNumber, value: a.SenseNumber);
                diff.Replace(path: x => x.JapaneseSentence!.Indices[i].Elements[j].SenseNumber, value: b.SenseNumber);
            }

            if (!string.Equals(a.SentenceForm, b.SentenceForm, StringComparison.Ordinal))
            {
                diff.Test(path: x => x.JapaneseSentence!.Indices[i].Elements[j].SentenceForm, value: a.SentenceForm);
                diff.Replace(path: x => x.JapaneseSentence!.Indices[i].Elements[j].SentenceForm, value: b.SentenceForm);
            }

            if (a.IsPriority != b.IsPriority)
            {
                diff.Test(path: x => x.JapaneseSentence!.Indices[i].Elements[j].IsPriority, value: a.IsPriority);
                diff.Replace(path: x => x.JapaneseSentence!.Indices[i].Elements[j].IsPriority, value: b.IsPriority);
            }
        }
        else if (a is not null)
        {
            diff.Test(path: x => x.JapaneseSentence!.Indices[i].Elements!, value: a, position: j);
            diff.Remove(path: x => x.JapaneseSentence!.Indices[i].Elements!, position: j);
        }
        else if (b is not null)
        {
            diff.Add(path: x => x.JapaneseSentence!.Indices[i].Elements!, value: b, position: j);
        }

        return diff.Operations;
    }
}
