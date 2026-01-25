/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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

using Microsoft.EntityFrameworkCore;
using Jitendex.MinimalJsonDiff;
using Jitendex.Tatoeba.Import.Models;

namespace Jitendex.Tatoeba.Import.SQLite;

internal static class Database
{
    private static readonly FileHeaderTable FileHeaderTable = new();
    private static readonly SequenceTable SequenceTable = new();
    private static readonly EntryTable EntryTable = new();
    private static readonly JapaneseSentenceTable JapaneseSentenceTable = new();
    private static readonly EnglishSentenceTable EnglishSentenceTable = new();
    private static readonly SegmentationTable SegmentationTable = new();
    private static readonly TokenTable TokenTable = new();

    public static void Initialize(Document document)
    {
        Console.Error.WriteLine($"Initializing database with data from {document.Header.Date:yyyy-MM-dd}");

        using var context = new Context();
        context.InitializeDatabase();
        context.ExecuteFastNewDatabasePragma();

        using (var transaction = context.Database.BeginTransaction())
        {
            FileHeaderTable.InsertItem(context, document.Header);
            SequenceTable.InsertItems(context, document.GetSequences());
            EntryTable.InsertItems(context, document.Entries.Values);
            JapaneseSentenceTable.InsertItems(context, document.JapaneseSentences.Values);
            EnglishSentenceTable.InsertItems(context, document.EnglishSentences.Values);
            SegmentationTable.InsertItems(context, document.Segmentations.Values);
            TokenTable.InsertItems(context, document.Tokens.Values);
            transaction.Commit();
        }

        context.SaveChanges();
    }

    public static void Update(DocumentDiff diff)
    {
        Console.Error.WriteLine($"Updating {diff.SequenceIds.Count} sequences with data from {diff.Date:yyyy-MM-dd}");

        using var context = new Context();
        var aSequences = DtoMapper.LoadSequencesWithoutRevisions(context, diff.SequenceIds);

        using (var transaction = context.Database.BeginTransaction())
        {
            context.ExecuteDeferForeignKeysPragma();

            FileHeaderTable.InsertItem(context, diff.InsertDocument.Header);
            SequenceTable.InsertOrIgnoreItems(context, diff.InsertDocument.GetSequences());

            EntryTable.InsertItems(context, diff.InsertDocument.Entries.Values);
            EnglishSentenceTable.InsertItems(context, diff.InsertDocument.EnglishSentences.Values);
            JapaneseSentenceTable.InsertItems(context, diff.InsertDocument.JapaneseSentences.Values);
            SegmentationTable.InsertItems(context, diff.InsertDocument.Segmentations.Values);
            TokenTable.InsertItems(context, diff.InsertDocument.Tokens.Values);

            EntryTable.UpdateItems(context, diff.UpdateDocument.Entries.Values);
            EnglishSentenceTable.UpdateItems(context, diff.UpdateDocument.EnglishSentences.Values);
            JapaneseSentenceTable.UpdateItems(context, diff.UpdateDocument.JapaneseSentences.Values);
            SegmentationTable.UpdateItems(context, diff.UpdateDocument.Segmentations.Values);
            TokenTable.UpdateItems(context, diff.UpdateDocument.Tokens.Values);

            TokenTable.DeleteItems(context, diff.DeleteDocument.Tokens.Values);
            SegmentationTable.DeleteItems(context, diff.DeleteDocument.Segmentations.Values);
            JapaneseSentenceTable.DeleteItems(context, diff.DeleteDocument.JapaneseSentences.Values);
            EnglishSentenceTable.DeleteItems(context, diff.DeleteDocument.EnglishSentences.Values);
            EntryTable.DeleteItems(context, diff.DeleteDocument.Entries.Values);

            transaction.Commit();
        }

        var bSequences = DtoMapper.LoadSequencesWithoutRevisions(context, diff.SequenceIds);

        var sequences = context.Sequences
            .Where(sequence => diff.SequenceIds.Contains(sequence.Id))
            .Include(static sequence => sequence.Revisions)
            .ToList();

        foreach (var sequence in sequences)
        {
            if (aSequences.TryGetValue(sequence.Id, out var aSequence))
            {
                var bSequence = bSequences[sequence.Id];
                var baDiff = JsonDiffer.Diff(a: bSequence, b: aSequence);
                sequence.Revisions.Add(new()
                {
                    SequenceId = sequence.Id,
                    Number = sequence.Revisions.Count,
                    CreatedDate = diff.Date,
                    DiffJson = baDiff,
                    Sequence = sequence,
                });
            }
        }

        context.SaveChanges();
    }
}
