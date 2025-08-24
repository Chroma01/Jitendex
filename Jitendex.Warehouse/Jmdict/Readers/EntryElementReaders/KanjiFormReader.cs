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
using Jitendex.Warehouse.Jmdict.Models;
using Jitendex.Warehouse.Jmdict.Models.EntryElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.KanjiFormElements;
using Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.KanjiFormElementReaders;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders;

internal class KanjiFormReader
{
    private readonly XmlReader Reader;
    private readonly EntityFactory Factory;
    private readonly InfoReader InfoReader;
    private readonly PriorityReader PriorityReader;

    public KanjiFormReader(XmlReader reader, EntityFactory factory)
    {
        Reader = reader;
        Factory = factory;
        InfoReader = new InfoReader(reader, factory);
        PriorityReader = new PriorityReader(reader, factory);
    }

    public async Task<KanjiForm> ReadAsync(Entry entry)
    {
        var kanjiForm = new KanjiForm
        {
            EntryId = entry.Id,
            Order = entry.KanjiForms.Count + 1,
            Text = string.Empty,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await Reader.ReadAsync())
        {
            switch (Reader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(kanjiForm);
                    break;
                case XmlNodeType.Text:
                    var text = await Reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{KanjiForm.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = Reader.Name == KanjiForm.XmlTagName;
                    break;
            }
        }
        return kanjiForm;
    }

    private async Task ReadChildElementAsync(KanjiForm kanjiForm)
    {
        switch (Reader.Name)
        {
            case "keb":
                kanjiForm.Text = await Reader.ReadElementContentAsStringAsync();
                break;
            case Info.XmlTagName:
                var info = await InfoReader.ReadAsync(kanjiForm);
                kanjiForm.Infos.Add(info);
                break;
            case Priority.XmlTagName:
                var priority = await PriorityReader.ReadAsync(kanjiForm);
                kanjiForm.Priorities.Add(priority);
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{Reader.Name}` found in element `{KanjiForm.XmlTagName}`");
        }
    }
}
