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
using Jitendex.KanjiVG.Models;

namespace Jitendex.KanjiVG.Database;

internal static class ComponentGroupStyleData
{
    // Column names
    private const string C1 = nameof(ComponentGroupStyle.Id);
    private const string C2 = nameof(ComponentGroupStyle.Text);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(ComponentGroupStyle)}"
        ("{C1}", "{C2}") VALUES
        ( {P1} ,  {P2} );
        """;

    public static async Task InsertComponentGroupStylesAsync(this Context db, List<ComponentGroupStyle> componentGroupStyles)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = InsertSql;

        foreach (var componentGroupStyle in componentGroupStyles)
        {
            command.Parameters.AddRange(new SqliteParameter[]
            {
                new(P1, componentGroupStyle.Id),
                new(P2, componentGroupStyle.Text),
            });

            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
        }
    }
}
