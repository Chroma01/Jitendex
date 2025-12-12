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

using Jitendex.Tatoeba.Dto;

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

        // Delete and recreate the database file.
        context.InitializeDatabase();

        // For faster importing, write data to memory rather than to the disk.
        context.ExecuteFastNewDatabasePragma();

        // Begin inserting data.
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

        // Write database to the disk.
        context.SaveChanges();
    }
}
