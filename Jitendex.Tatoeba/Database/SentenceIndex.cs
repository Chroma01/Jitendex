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

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Jitendex.Tatoeba.Models;

namespace Jitendex.Tatoeba.Database;

internal static class SentenceIndexData
{
    // Column names
    private const string C1 = nameof(SentenceIndex.SentenceId);
    private const string C2 = nameof(SentenceIndex.Order);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(SentenceIndex)}"
        ("{C1}", "{C2}") VALUES
        ( {P1} ,  {P2} );
        """;

    public static async Task InsertIndicesAsync(this Context db, ICollection<SentenceIndex> indices)
    {
        var allJapaneseSentences = new Dictionary<int, JapaneseSentence>(indices.Count);
        var allIndexElements = new List<IndexElement>(indices.Count * 8);

        await using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = InsertSql;

            foreach (var index in indices)
            {
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new(P1, index.SentenceId),
                    new(P2, index.Order),
                });

                var commandExecution = command.ExecuteNonQueryAsync();

                if (!allJapaneseSentences.ContainsKey(index.SentenceId))
                {
                    allJapaneseSentences[index.SentenceId] = index.Sentence;
                }
                allIndexElements.AddRange(index.Elements);

                await commandExecution;
                command.Parameters.Clear();
            }
        }

        await db.InsertJapaneseSentencesAsync(allJapaneseSentences.Values);
        await db.InsertIndexElementsAsync(allIndexElements);
    }
}
