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

using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

[PrimaryKey(nameof(TypeName), nameof(Key))]
public class ExampleSource
{
    public required string TypeName { get; set; }
    public required int Key { get; set; }
    public required string Text { get; set; }
    public required string Translation { get; set; }

    internal const string XmlTagName = "ex_srce";

    private static readonly Dictionary<(string, int), ExampleSource> Cache = [];

    internal static ExampleSource FindByPrimaryKey(string typeName, int key)
    {
        var primaryKey = (typeName, key);
        if (Cache.TryGetValue(primaryKey, out ExampleSource? source))
            return source;
        var description = string.Empty;
        var newSource = new ExampleSource
        {
            TypeName = typeName,
            Key = key,
            Text = string.Empty,
            Translation = string.Empty,
        };
        Cache.Add(primaryKey, newSource);
        return newSource;
    }
}
