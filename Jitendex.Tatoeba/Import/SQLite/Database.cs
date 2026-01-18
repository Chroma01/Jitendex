/*
Copyright (c) 2025 Stephen Kraus
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
    private static readonly JapaneseSentenceTable JapaneseSentenceTable = new();
    private static readonly EnglishSentenceTable EnglishSentenceTable = new();
    private static readonly TokenizedSentenceTable TokenizedSentenceTable = new();
    private static readonly TokenTable TokenTable = new();

    public static void Initialize(Document document)
    {
        Console.Error.WriteLine($"Initializing database with data from {document.Date:yyyy-MM-dd}");

        using var context = new Context();
        context.InitializeDatabase();
        context.ExecuteFastNewDatabasePragma();

        using (var transaction = context.Database.BeginTransaction())
        {
            FileHeaderTable.InsertItem(context, document.GetFileHeader());
            SequenceTable.InsertItems(context, document.GetSequences());
            JapaneseSentenceTable.InsertItems(context, document.JapaneseSequences.Values);
            EnglishSentenceTable.InsertItems(context, document.EnglishSequences.Values);
            TokenizedSentenceTable.InsertItems(context, document.TokenizedSentences.Values);
            TokenTable.InsertItems(context, document.Tokens.Values);
            transaction.Commit();
        }

        context.SaveChanges();
    }

    public static void Update(DocumentDiff diff)
    {
        Console.Error.WriteLine($"Updating {diff.SequenceIds.Count} sequences with data from {diff.Date:yyyy-MM-dd}");

        using var context = new Context();
        var aSequences = LoadSequences(context, diff.SequenceIds);

        using (var transaction = context.Database.BeginTransaction())
        {
            context.ExecuteDeferForeignKeysPragma();

            FileHeaderTable.InsertItem(context, diff.InsertDocument.GetFileHeader());
            SequenceTable.InsertOrIgnoreItems(context, diff.InsertDocument.GetSequences());

            EnglishSentenceTable.InsertItems(context, diff.InsertDocument.EnglishSequences.Values);
            JapaneseSentenceTable.InsertItems(context, diff.InsertDocument.JapaneseSequences.Values);
            TokenizedSentenceTable.InsertItems(context, diff.InsertDocument.TokenizedSentences.Values);
            TokenTable.InsertItems(context, diff.InsertDocument.Tokens.Values);

            EnglishSentenceTable.UpdateItems(context, diff.UpdateDocument.EnglishSequences.Values);
            JapaneseSentenceTable.UpdateItems(context, diff.UpdateDocument.JapaneseSequences.Values);
            TokenizedSentenceTable.UpdateItems(context, diff.UpdateDocument.TokenizedSentences.Values);
            TokenTable.UpdateItems(context, diff.UpdateDocument.Tokens.Values);

            TokenTable.DeleteItems(context, diff.DeleteDocument.Tokens.Values);
            TokenizedSentenceTable.DeleteItems(context, diff.DeleteDocument.TokenizedSentences.Values);
            JapaneseSentenceTable.DeleteItems(context, diff.DeleteDocument.JapaneseSequences.Values);
            EnglishSentenceTable.DeleteItems(context, diff.DeleteDocument.EnglishSequences.Values);

            transaction.Commit();
        }

        var bSequences = LoadSequences(context, diff.SequenceIds);

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

    private static Dictionary<int, Entities.Sequence> LoadSequences(Context context, IReadOnlySet<int> ids)
        => context.Sequences.AsNoTracking()
        .Where(sequence => ids.Contains(sequence.Id))
        .Include(static sequence => sequence.EnglishSentence)
        .Include(static sequence => sequence.JapaneseSentence)
        .ThenInclude(static japaneseSentence => japaneseSentence!.TokenizedSentences)
        .ThenInclude(static tokenizedSentence => tokenizedSentence.Tokens)
        .ToDictionary(static sequence => sequence.Id);
}
