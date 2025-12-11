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

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Jitendex.Tatoeba.Models;

namespace Jitendex.Tatoeba.SQLite;

internal static class Database
{
    private static readonly DocumentMetadataTable DocumentMetadataTable = new();
    private static readonly SequenceTable SequenceTable = new();
    private static readonly JapaneseSentenceTable JapaneseSentenceTable = new();
    private static readonly EnglishSentenceTable EnglishSentenceTable = new();
    private static readonly SentenceIndexTable SentenceIndexTable = new();
    private static readonly IndexElementTable IndexElementTable = new();

    public static void Initialize(Document document)
    {
        using var context = new Context();

        // Delete and recreate the database file.
        context.InitializeDatabase();

        // For faster importing, write data to memory rather than to the disk.
        context.ExecuteFastNewDatabasePragma();

        // Begin inserting data.
        using (var transaction = context.Database.BeginTransaction())
        {
            DocumentMetadataTable.InsertItem(context, document.Metadata);
            SequenceTable.InsertItems(context, document.Sequences.Values);
            JapaneseSentenceTable.InsertItems(context, document.JapaneseSentences.Values);
            EnglishSentenceTable.InsertItems(context, document.EnglishSentences.Values);
            SentenceIndexTable.InsertItems(context, document.SentenceIndices.Values);
            IndexElementTable.InsertItems(context, document.IndexElements.Values);
            transaction.Commit();
        }

        // Write database to the disk.
        context.SaveChanges();
    }

    public static void Update(Document docA, Document docB)
    {
        var diff = new DocumentDiff(docA, docB);

        using var context = new Context();
        using var transaction = context.Database.BeginTransaction();

        Console.Error.WriteLine($"There are {diff.BAPatches.Keys.Count} keys to process");

        var keys = new HashSet<int>(diff.BAPatches.Keys);

        var existingSequences = context.Sequences
            .Where(s => keys.Contains(s.Id))
            .Include(s => s.Revisions)
            .ToDictionary(s => s.Id);

        Console.Error.WriteLine($"Retrieved {existingSequences.Count} entities from the database");
        var options = new JsonSerializerOptions { WriteIndented = true };

        foreach (var (key, patch) in diff.BAPatches)
        {
            if (existingSequences.TryGetValue(key, out var sequence))
            {
                var revision = new Revision
                {
                    SequenceId = key,
                    Number = sequence.Revisions.Count,
                    CreatedDate = diff.Date,
                    DiffJson = JsonSerializer.Serialize(patch, options),
                    Sequence = sequence,
                };
                sequence.Revisions.Add(revision);
            }
            else
            {
                var newSequence = new Sequence
                {
                    Id = key,
                    CreatedDate = diff.Date,
                };
                context.Sequences.Add(newSequence);
            }
        }

        context.SaveChanges();

        var updateSequences = context.Sequences
            .Where(sequence => keys.Contains(sequence.Id))
            .Include(sequence => sequence.EnglishSentence)
            .ThenInclude(englishSentence => englishSentence!.Indices)
            .Include(sequence => sequence.JapaneseSentence)
            .ThenInclude(japaneseSentence => japaneseSentence!.Indices)
            .ThenInclude(index => index.Elements)
            .ToList();

        if (updateSequences.Count != keys.Count)
        {
            throw new Exception();
        }

        foreach (var sequence in updateSequences)
        {
            if (!docB.Sequences.TryGetValue(sequence.Id, out var newSequence))
            {
                newSequence = new Sequence { Id = sequence.Id, CreatedDate = diff.Date };
            }

            if (newSequence.EnglishSentence is null)
            {
                if (sequence.EnglishSentence is not null)
                {
                    context.Remove(sequence.EnglishSentence);
                    sequence.EnglishSentence = null;
                }
            }
            else if (sequence.EnglishSentence is null)
            {
                sequence.EnglishSentence = new EnglishSentence
                {
                    SequenceId = sequence.Id,
                    Text = newSequence.EnglishSentence.Text,
                    Sequence = sequence,
                };
            }
            else
            {
                sequence.EnglishSentence.Text = newSequence.EnglishSentence.Text;
            }

            if (newSequence.JapaneseSentence is null)
            {
                if (sequence.JapaneseSentence is not null)
                {
                    context.Remove(sequence.JapaneseSentence);
                    sequence.JapaneseSentence = null;
                }
            }
            else if (sequence.JapaneseSentence is null)
            {
                sequence.JapaneseSentence = new JapaneseSentence
                {
                    SequenceId = sequence.Id,
                    Text = newSequence.JapaneseSentence.Text,
                    Sequence = sequence,
                };
            }
            else
            {
                sequence.JapaneseSentence.Text = newSequence.JapaneseSentence.Text;
            }

        }

        foreach (var sequence in updateSequences)
        {
            if (sequence.JapaneseSentence is null)
            {
                continue;
            }

            var newSequence = docB.Sequences[sequence.Id];
            var newIndicesLength = newSequence.JapaneseSentence!.Indices.Count;

            for (int i = 0; i < newIndicesLength; i++)
            {
                var newIndex = newSequence.JapaneseSentence.Indices[i];

                if (sequence.JapaneseSentence.Indices.ElementAtOrDefault(i) is SentenceIndex index)
                {
                    index.Meaning = context.Find<EnglishSentence>(newIndex.MeaningId)!;
                }
                else
                {
                    index = new SentenceIndex
                    {
                        SentenceId = sequence.Id,
                        Order = i + 1,
                        MeaningId = newIndex.MeaningId,
                        Meaning = context.Find<EnglishSentence>(newIndex.MeaningId)!,
                        Sentence = sequence.JapaneseSentence,
                    };
                    context.SentenceIndices.Add(index);
                    sequence.JapaneseSentence.Indices.Add(index);
                }

                var newElementsLength = newIndex.Elements.Count;

                for (int j = 0; j < newElementsLength; j++)
                {
                    var newElement = newIndex.Elements[j];
                    if (index.Elements.ElementAtOrDefault(j) is IndexElement element)
                    {
                        element.Headword = newElement.Headword;
                        element.Reading = newElement.Reading;
                        element.EntryId = newElement.EntryId;
                        element.SenseNumber = newElement.SenseNumber;
                        element.SentenceForm = newElement.SentenceForm;
                        element.IsPriority = newElement.IsPriority;
                    }
                    else
                    {
                        element = new IndexElement
                        {
                            SentenceId = index.SentenceId,
                            IndexOrder = index.Order,
                            Order = j + 1,
                            Headword = newElement.Headword,
                            Reading = newElement.Reading,
                            EntryId = newElement.EntryId,
                            SenseNumber = newElement.SenseNumber,
                            SentenceForm = newElement.SentenceForm,
                            IsPriority = newElement.IsPriority,
                            Index = index,
                        };
                        context.IndexElements.Add(element);
                        index.Elements.Add(element);
                    }
                }
                for (int j = newElementsLength; j < index.Elements.Count; j++)
                {
                    var element = index.Elements[j];
                    context.Remove(element);
                }
            }
            for (int i = newIndicesLength; i < sequence.JapaneseSentence.Indices.Count; i++)
            {
                var index = sequence.JapaneseSentence.Indices[i];
                context.Remove(index);
            }
        }

        context.Metadata.Add(docB.Metadata);

        // Write database to the disk.
        context.SaveChanges();
        transaction.Commit();
    }
}
