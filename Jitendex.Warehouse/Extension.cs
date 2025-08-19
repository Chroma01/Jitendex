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

using System.Xml;

namespace Jitendex.Warehouse;

public static class Extension
{
    public async static Task<string> ReadAndGetTextValueAsync(this XmlReader reader)
    {
        if (reader.NodeType != XmlNodeType.Element)
            throw new Exception($"Node `{reader.Name}` is not an element.");
        if (reader.IsEmptyElement)
            throw new Exception($"Tag `{reader.Name}` is empty.");
        var tagName = reader.Name;
        var text = string.Empty;
        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    throw new Exception($"Unexpected element named `{reader.Name}` found in tag `{tagName}`");
                case XmlNodeType.Text:
                    text = await reader.GetValueAsync();
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == tagName;
                    break;
            }
        }
        return text;
    }
}