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
using Jitendex.JMdict.Models.EntryElements.SenseElements;

namespace Jitendex.JMdict.Data.EntryElements.SenseElements;

internal static class LanguageSourceData
{
    // Column names
    private const string C1 = nameof(LanguageSource.EntryId);
    private const string C2 = nameof(LanguageSource.SenseOrder);
    private const string C3 = nameof(LanguageSource.Order);
    private const string C4 = nameof(LanguageSource.Text);
    private const string C5 = nameof(LanguageSource.LanguageCode);
    private const string C6 = nameof(LanguageSource.TypeName);
    private const string C7 = nameof(LanguageSource.IsWasei);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";
    private const string P4 = $"@{C4}";
    private const string P5 = $"@{C5}";
    private const string P6 = $"@{C6}";
    private const string P7 = $"@{C7}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(LanguageSource)}"
        ("{C1}", "{C2}", "{C3}", "{C4}", "{C5}", "{C6}", "{C7}") VALUES
        ( {P1} ,  {P2} ,  {P3} ,  {P4} ,  {P5} ,  {P6} ,  {P7} );
        """;

    public static async Task InsertLanguageSources(this JmdictContext db, List<LanguageSource> languageSources)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = InsertSql;

        foreach (var languageSource in languageSources)
        {
            command.Parameters.AddRange(new SqliteParameter[]
            {
                new(P1, languageSource.EntryId),
                new(P2, languageSource.SenseOrder),
                new(P3, languageSource.Order),
                new(P4, languageSource.Text is null ? DBNull.Value : languageSource.Text),
                new(P5, languageSource.LanguageCode),
                new(P6, languageSource.TypeName),
                new(P7, languageSource.IsWasei),
            });

            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
        }
    }
}
