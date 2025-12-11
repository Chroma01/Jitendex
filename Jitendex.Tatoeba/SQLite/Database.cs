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

    public static void Update(Document document, DocumentDiff diff)
    {
        using var context = new Context();
        context.ExecuteFastNewDatabasePragma();
        using var transaction = context.Database.BeginTransaction();

        var keys = new HashSet<int>(diff.BAPatches.Keys);
        Console.Error.WriteLine($"There are {keys.Count} keys to process");

        var sequences = context.Sequences
            .Where(s => keys.Contains(s.Id))
            .Include(s => s.Revisions)
            .ToDictionary(s => s.Id);

        Console.Error.WriteLine($"Retrieved {sequences.Count} entities from the database");
        var options = new JsonSerializerOptions { WriteIndented = true };

        foreach (var (key, patch) in diff.BAPatches)
        {
            if (sequences.TryGetValue(key, out var sequence))
            {
                var reversePatch = diff.BAPatches[key];
                var revision = new Revision
                {
                    SequenceId = key,
                    Number = sequence.Revisions.Count,
                    CreatedDate = diff.Date,
                    DiffJson = JsonSerializer.Serialize(reversePatch, options),
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
        context.ExecuteDeferForeignKeysPragma();

        DocumentMetadataTable.InsertItem(context, document.Metadata);

        EnglishSentenceTable.Truncate(context);
        EnglishSentenceTable.InsertItems(context, document.EnglishSentences.Values);

        JapaneseSentenceTable.Truncate(context);
        JapaneseSentenceTable.InsertItems(context, document.JapaneseSentences.Values);

        SentenceIndexTable.Truncate(context);
        SentenceIndexTable.InsertItems(context, document.SentenceIndices.Values);

        IndexElementTable.Truncate(context);
        IndexElementTable.InsertItems(context, document.IndexElements.Values);

        // Write database to the disk.
        context.SaveChanges();
        transaction.Commit();
    }
}
