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
using Microsoft.Extensions.Logging;
using Jitendex.Warehouse.Kanjidic2.Models;
using Jitendex.Warehouse.Kanjidic2.Models.Groups;
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Readers.GroupReaders;

internal class MiscGroupReader
{
    private readonly ILogger<MiscGroupReader> _logger;
    private readonly XmlReader _xmlReader;

    public MiscGroupReader(ILogger<MiscGroupReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task<MiscGroup> ReadAsync(Entry entry)
    {
        var miscGroup = new MiscGroup
        {
            Character = entry.Character,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(miscGroup);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, MiscGroup.XmlTagName, text);
                    entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == MiscGroup.XmlTagName;
                    break;
            }
        }
        return miscGroup;
    }

    private async Task ReadChildElementAsync(MiscGroup group)
    {
        switch (_xmlReader.Name)
        {
            case "grade":
                group.Grade = int.Parse(await _xmlReader.ReadElementContentAsStringAsync());
                break;
            case "freq":
                group.Frequency = int.Parse(await _xmlReader.ReadElementContentAsStringAsync());
                break;
            case "jlpt":
                group.JlptLevel = int.Parse(await _xmlReader.ReadElementContentAsStringAsync());
                break;
            case StrokeCount.XmlTagName:
                group.StrokeCounts.Add(new StrokeCount
                {
                    Character = group.Character,
                    Order = group.StrokeCounts.Count + 1,
                    Value = int.Parse(await _xmlReader.ReadElementContentAsStringAsync()),
                    Entry = group.Entry,
                });
                break;
            case Variant.XmlTagName:
                group.Variants.Add(new Variant
                {
                    Character = group.Character,
                    Order = group.Variants.Count + 1,
                    Type = _xmlReader.GetAttribute("var_type") ?? throw new Exception($"Character `{group.Character}` missing variant type"),
                    Text = await _xmlReader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            case RadicalName.XmlTagName:
                group.RadicalNames.Add(new RadicalName
                {
                    Character = group.Character,
                    Order = group.RadicalNames.Count + 1,
                    Text = await _xmlReader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, MiscGroup.XmlTagName);
                group.Entry.IsCorrupt = true;
                return;
        }
    }
}
