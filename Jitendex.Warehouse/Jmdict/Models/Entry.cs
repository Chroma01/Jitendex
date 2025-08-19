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
using Jitendex.Warehouse.Jmdict.Models.EntryElements;

namespace Jitendex.Warehouse.Jmdict.Models;

public class Entry
{
    public required int Id { get; set; }
    public List<Reading> Readings { get; set; } = [];
    public List<KanjiForm> KanjiForms { get; set; } = [];
    public List<Sense> Senses { get; set; } = [];

    #region Static XML Factory

    public const string XmlTagName = "entry";

    public async static Task<Entry> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta)
    {
        if (docMeta == null)
            throw new ArgumentNullException(nameof(docMeta),
                "Metadata is null. It should have been parsed from the JMdict XML before any of the entries.");

        var entry = new Entry { Id = -1 };

        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    await ProcessElementAsync(reader, docMeta, entry);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return PostProcess(entry);
    }

    private async static Task ProcessElementAsync(XmlReader reader, DocumentMetadata docMeta, Entry entry)
    {
        switch (reader.Name)
        {
            case "ent_seq":
                var sequenceText = await reader.ReadAndGetTextValueAsync();
                entry.Id = int.Parse(sequenceText);
                break;
            case KanjiForm.XmlTagName:
                var kanjiForm = await KanjiForm.FromXmlAsync(reader, docMeta, entry);
                entry.KanjiForms.Add(kanjiForm);
                break;
            case Reading.XmlTagName:
                var reading = await Reading.FromXmlAsync(reader, docMeta, entry);
                entry.Readings.Add(reading);
                break;
            case Sense.XmlTagName:
                var sense = await Sense.FromXmlAsync(reader, docMeta, entry);
                if (sense.Glosses.Any(g => g.Language == "eng"))
                {
                    entry.Senses.Add(sense);
                }
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{XmlTagName}`");
        }
    }

    private static Entry PostProcess(Entry entry)
    {
        BridgeReadingsAndKanjiForms(entry);
        // Anticipating more operations here later.
        return entry;
    }

    private static void BridgeReadingsAndKanjiForms(Entry entry)
    {
        foreach (var reading in entry.Readings)
        {
            if (reading.NoKanji)
                continue;
            if (reading.InfoTags.Any(x => x.TagId == "sk"))
                continue;

            foreach (var kanjiForm in entry.KanjiForms)
            {
                if (kanjiForm.InfoTags.Any(x => x.TagId == "sK"))
                    continue;
                if (reading.ConstraintKanjiFormTexts.Count > 0 && !reading.ConstraintKanjiFormTexts.Contains(kanjiForm.Text))
                    continue;
                var bridge = new ReadingKanjiFormBridge
                {
                    EntryId = entry.Id,
                    ReadingOrder = reading.Order,
                    KanjiFormOrder = kanjiForm.Order,
                    Reading = reading,
                    KanjiForm = kanjiForm,
                };
                reading.KanjiFormBridges.Add(bridge);
                kanjiForm.ReadingBridges.Add(bridge);
            }
        }
    }

    #endregion
}
