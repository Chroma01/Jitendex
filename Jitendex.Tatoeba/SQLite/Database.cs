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
using Json.Patch;
using Microsoft.EntityFrameworkCore;
using Jitendex.Tatoeba.ImportDto;

namespace Jitendex.Tatoeba.SQLite;

internal static class Database
{
    private static readonly DocumentMetadataTable DocumentMetadataTable = new();
    private static readonly SequenceTable SequenceTable = new();
    private static readonly JapaneseSentenceTable JapaneseSentenceTable = new();
    private static readonly EnglishSentenceTable EnglishSentenceTable = new();
    private static readonly TokenizedSentenceTable TokenizedSentenceTable = new();
    private static readonly TokenTable TokenTable = new();

    public static void Initialize(Document document)
    {
        using var context = new Context();
        context.InitializeDatabase();
        context.ExecuteFastNewDatabasePragma();

        using (var transaction = context.Database.BeginTransaction())
        {
            DocumentMetadataTable.InsertItem(context, document.GetMetadata());
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
        using var context = new Context();

        var ids = diff.GetTouchedSequenceIds();

        var oldSequences = context.Sequences
            .AsNoTracking()
            .Where(sequence => ids.Contains(sequence.Id))
            .Include(sequence => sequence.EnglishSentence)
            .Include(sequence => sequence.JapaneseSentence)
            .ThenInclude(japaneseSentence => japaneseSentence!.TokenizedSentences)
            .ThenInclude(tokenizedSentence => tokenizedSentence.Tokens)
            .ToDictionary(s => s.Id);

        context.ExecuteDeferForeignKeysPragma();

        using (var transaction = context.Database.BeginTransaction())
        {
            DocumentMetadataTable.InsertItem(context, diff.InsertDocument.GetMetadata());
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

        var newSequences = context.Sequences
            .AsNoTracking()
            .Where(sequence => ids.Contains(sequence.Id))
            .Include(sequence => sequence.EnglishSentence)
            .Include(sequence => sequence.JapaneseSentence)
            .ThenInclude(japaneseSentence => japaneseSentence!.TokenizedSentences)
            .ThenInclude(tokenizedSentence => tokenizedSentence.Tokens)
            .ToList();

        var patches = new Dictionary<int, string>(ids.Count);
        var options = new JsonSerializerOptions { WriteIndented = true };

        foreach (var newSeq in newSequences)
        {
            if (oldSequences.TryGetValue(newSeq.Id, out var oldSeq))
            {
                var original = JsonSerializer.SerializeToNode(newSeq);
                var target = JsonSerializer.SerializeToNode(oldSeq);
                var patch = original.CreatePatch(target);
                patches.Add(newSeq.Id, JsonSerializer.Serialize(patch, options));
            }
        }

        var sequences = context.Sequences
            .Where(sequence => ids.Contains(sequence.Id))
            .Include(sequence => sequence.Revisions)
            .ToList();

        foreach (var sequence in sequences)
        {
            if (patches.TryGetValue(sequence.Id, out var patch))
            {
                sequence.Revisions.Add(new()
                {
                    SequenceId = sequence.Id,
                    Number = sequence.Revisions.Count,
                    CreatedDate = diff.Date,
                    DiffJson = patch,
                    Sequence = sequence,
                });
            }
        }

        context.SaveChanges();
    }
}
