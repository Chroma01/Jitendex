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
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Models.Groups;

internal static class QueryCodeGroupReader
{
    public async static Task<QueryCodeGroup> ReadQueryCodeGroupAsync(this XmlReader reader, Entry entry)
    {
        var queryCodeGroup = new QueryCodeGroup
        {
            Character = entry.Character,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    await reader.ReadChildElementAsync(queryCodeGroup);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{QueryCodeGroup.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == QueryCodeGroup.XmlTagName;
                    break;
            }
        }
        return queryCodeGroup;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, QueryCodeGroup group)
    {
        switch (reader.Name)
        {
            case QueryCode.XmlTagName:
                group.QueryCodes.Add(new QueryCode
                {
                    Character = group.Character,
                    Order = group.QueryCodes.Count + 1,
                    Type = reader.GetAttribute("qc_type") ?? throw new Exception($"Character `{group.Character}` missing query code type"),
                    Misclassification = reader.GetAttribute("skip_misclass"),
                    Text = await reader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{QueryCodeGroup.XmlTagName}`");
        }
    }
}
