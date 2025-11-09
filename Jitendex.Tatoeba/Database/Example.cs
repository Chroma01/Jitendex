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

internal static class ExampleData
{
    // Column names
    private const string C1 = nameof(Example.JapaneseSentenceId);
    private const string C2 = nameof(Example.EnglishSentenceId);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(Example)}"
        ("{C1}", "{C2}") VALUES
        ( {P1} ,  {P2} );
        """;

    public static async Task InsertExamplesAsync(this Context db, ICollection<Example> examples)
    {
        var allEnglishSentences = new Dictionary<int, EnglishSentence>(examples.Count);

        await using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = InsertSql;

            foreach (var example in examples)
            {
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new(P1, example.JapaneseSentenceId),
                    new(P2, example.EnglishSentenceId),
                });

                var commandExecution = command.ExecuteNonQueryAsync();

                if (!allEnglishSentences.ContainsKey(example.EnglishSentenceId))
                {
                    allEnglishSentences[example.EnglishSentenceId] = example.EnglishSentence;
                }

                await commandExecution;
                command.Parameters.Clear();
            }
        }

        await db.InsertEnglishSentencesAsync(allEnglishSentences.Values);
    }
}
