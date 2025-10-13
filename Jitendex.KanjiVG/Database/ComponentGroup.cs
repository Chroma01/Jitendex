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
using Microsoft.Extensions.Configuration;

namespace Jitendex.KanjiVG.Database;

internal static class ComponentGroupData
{
    // Column names
    private const string C1 = nameof(ComponentGroup.UnicodeScalarValue);
    private const string C2 = nameof(ComponentGroup.VariantTypeName);
    private const string C3 = nameof(ComponentGroup.Id);
    private const string C4 = nameof(ComponentGroup.Style);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";
    private const string P4 = $"@{C4}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(ComponentGroup)}"
        ("{C1}", "{C2}", "{C3}", "{C4}") VALUES
        ( {P1} ,  {P2} ,  {P3} ,  {P4} );
        """;

    public static async Task InsertComponentGroupsAsync(this Context db, List<ComponentGroup> componentGroups)
    {
        var allComponents = new List<Component>(componentGroups.Count * 8);

        await using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = InsertSql;

            foreach (var componentGroup in componentGroups)
            {
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new(P1, componentGroup.UnicodeScalarValue),
                    new(P2, componentGroup.VariantTypeName),
                    new(P3, componentGroup.Id),
                    new(P4, componentGroup.Style),
                });

                var commandExecution = command.ExecuteNonQueryAsync();

                allComponents.AddRange(EnumerateComponents(componentGroup.Components));

                await commandExecution;
                command.Parameters.Clear();
            }
        }

        await db.InsertComponentsAsync(allComponents);
    }

    public static IEnumerable<Component> EnumerateComponents(List<Component> components)
    {
        foreach (var component in components)
        {
            yield return component;
            foreach (var childComponent in EnumerateComponents(component.Children))
            {
                yield return childComponent;
            }
        }
    }
}
