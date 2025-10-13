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

internal static class ComponentData
{
    // Column names
    private const string C1 = nameof(Component.UnicodeScalarValue);
    private const string C2 = nameof(Component.VariantTypeName);
    private const string C3 = nameof(Component.GlobalOrder);
    private const string C4 = nameof(Component.ParentGlobalOrder);
    private const string C5 = nameof(Component.LocalOrder);
    private const string C6 = nameof(Component.Text);
    private const string C7 = nameof(Component.Variant);
    private const string C8 = nameof(Component.Partial);
    private const string C9 = nameof(Component.Original);
    private const string C10 = nameof(Component.Part);
    private const string C11 = nameof(Component.Number);
    private const string C12 = nameof(Component.TradForm);
    private const string C13 = nameof(Component.RadicalForm);
    private const string C14 = nameof(Component.Position);
    private const string C15 = nameof(Component.Radical);
    private const string C16 = nameof(Component.Phon);

    // Parameter names
    private const string P1 = $"@{C1}";
    private const string P2 = $"@{C2}";
    private const string P3 = $"@{C3}";
    private const string P4 = $"@{C4}";
    private const string P5 = $"@{C5}";
    private const string P6 = $"@{C6}";
    private const string P7 = $"@{C7}";
    private const string P8 = $"@{C8}";
    private const string P9 = $"@{C9}";
    private const string P10 = $"@{C10}";
    private const string P11 = $"@{C11}";
    private const string P12 = $"@{C12}";
    private const string P13 = $"@{C13}";
    private const string P14 = $"@{C14}";
    private const string P15 = $"@{C15}";
    private const string P16 = $"@{C16}";

    private const string InsertSql =
        $"""
        INSERT INTO "{nameof(Component)}"
        ("{C1}", "{C2}", "{C3}", "{C4}", "{C5}", "{C6}", "{C7}", "{C8}", "{C9}", "{C10}", "{C11}", "{C12}", "{C13}", "{C14}", "{C15}", "{C16}") VALUES
        ( {P1} ,  {P2} ,  {P3} ,  {P4} ,  {P5} ,  {P6} ,  {P7} ,  {P8} ,  {P9} ,  {P10} ,  {P11} ,  {P12} ,  {P13} ,  {P14} ,  {P15} ,  {P16} );
        """;

    public static async Task InsertComponentsAsync(this Context db, List<Component> components)
    {
        var allStrokes = new List<Stroke>(components.Count * 2);

        await using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = InsertSql;

            foreach (var component in components)
            {
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new(P1, component.UnicodeScalarValue),
                    new(P2, component.VariantTypeName),
                    new(P3, component.GlobalOrder),
                    new(P4, component.ParentGlobalOrder is null ? DBNull.Value : component.ParentGlobalOrder),
                    new(P5, component.LocalOrder),
                    new(P6, component.Text is null ? DBNull.Value : component.Text),
                    new(P7, component.Variant),
                    new(P8, component.Partial),
                    new(P9, component.Original is null ? DBNull.Value : component.Original),
                    new(P10, component.Part is null ? DBNull.Value : component.Part),
                    new(P11, component.Number is null ? DBNull.Value : component.Number),
                    new(P12, component.TradForm),
                    new(P13, component.RadicalForm),
                    new(P14, component.Position is null ? DBNull.Value : component.Position),
                    new(P15, component.Radical is null ? DBNull.Value : component.Radical),
                    new(P16, component.Phon is null ? DBNull.Value : component.Phon),
                });

                var commandExecution = command.ExecuteNonQueryAsync();

                allStrokes.AddRange(component.Strokes);

                await commandExecution;
                command.Parameters.Clear();
            }
        }

        await db.InsertStrokesAsync(allStrokes);
    }
}
