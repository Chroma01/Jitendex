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

internal class DictionaryGroupReader
{
    private readonly XmlReader _xmlReader;
    private readonly ILogger<DictionaryGroupReader> _logger;

    public DictionaryGroupReader(XmlReader xmlReader, ILogger<DictionaryGroupReader> logger) =>
        (_xmlReader, _logger) =
        (@xmlReader, @logger);

    public async Task<DictionaryGroup> ReadAsync(Entry entry)
    {
        var dicNumberGroup = new DictionaryGroup
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
                    await ReadChildElementAsync(dicNumberGroup);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{DictionaryGroup.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == DictionaryGroup.XmlTagName;
                    break;
            }
        }
        return dicNumberGroup;
    }

    private async Task ReadChildElementAsync(DictionaryGroup group)
    {
        switch (_xmlReader.Name)
        {
            case Dictionary.XmlTagName:
                var volume = _xmlReader.GetAttribute("m_vol");
                var page = _xmlReader.GetAttribute("m_page");
                group.Dictionaries.Add(new Dictionary
                {
                    Character = group.Character,
                    Order = group.Dictionaries.Count + 1,
                    Type = _xmlReader.GetAttribute("dr_type") ?? throw new Exception($"Character `{group.Character}` missing dictionary type"),
                    Volume = volume != null ? int.Parse(volume) : null,
                    Page = page != null ? int.Parse(page) : null,
                    Text = await _xmlReader.ReadElementContentAsStringAsync(),
                    Entry = group.Entry,
                });
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{_xmlReader.Name}` found in element `{DictionaryGroup.XmlTagName}`");
        }
    }
}
