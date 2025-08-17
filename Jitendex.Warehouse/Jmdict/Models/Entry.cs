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

using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Jitendex.Warehouse.Jmdict.Models;

[Table("Jmdict.Entries")]
public class Entry
{
    public required int Id { get; set; }
    public required List<Reading> Readings { get; set; }
    public List<KanjiForm>? KanjiForms { get; set; }

    #region Static XML Factory

    public const string XmlTagName = "entry";

    public async static Task<Entry> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta)
    {
        var entry = new Entry
        {
            Id = -1,
            Readings = [],
        };
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    await ProcessElementAsync(reader, docMeta, currentTagName, entry);
                    break;
                case XmlNodeType.Text:
                    await ProcessTextAsync(reader, currentTagName, entry);
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return PostProcess(entry);
    }

    private async static Task ProcessElementAsync(XmlReader reader, DocumentMetadata docMeta, string tagName, Entry entry)
    {
        switch (tagName)
        {
            case KanjiForm.XmlTagName:
                var kanjiForm = await KanjiForm.FromXmlAsync(reader, entry, docMeta);
                entry.KanjiForms ??= [];
                entry.KanjiForms.Add(kanjiForm);
                break;
            case Reading.XmlTagName:
                var reading = await Reading.FromXmlAsync(reader, entry, docMeta);
                entry.Readings.Add(reading);
                break;
            default:
                // TODO: Log warning.
                break;
        }
    }

    private async static Task ProcessTextAsync(XmlReader reader, string tagName, Entry entry)
    {
        switch (tagName)
        {
            case "ent_seq":
                var text = await reader.GetValueAsync();
                if (int.TryParse(text, out int sequence))
                {
                    entry.Id = sequence;
                }
                else
                {
                    // TODO: Log warning.
                }
                break;
            default:
                // TODO: Log warning.
                break;
        }
    }

    private static Entry PostProcess(Entry entry)
    {
        BridgeReadingsAndKanjiForms(entry);
        return entry;
    }

    private static void BridgeReadingsAndKanjiForms(Entry entry)
    {
        foreach (var reading in entry.Readings)
        {
            if (reading.NoKanji)
                continue;
            if (reading.InfoTags?.Any(i => i == "sk") ?? false)
                continue;

            foreach (var kanjiForm in entry.KanjiForms ?? [])
            {
                if (kanjiForm.InfoTags?.Any(i => i == "sK") ?? false)
                    continue;
                if (reading.ConstraintKanjiFormTexts != null && !reading.ConstraintKanjiFormTexts.Contains(kanjiForm.Text))
                    continue;
                var bridge = new ReadingKanjiBridge
                {
                    EntryId = entry.Id,
                    ReadingOrder = reading.Order,
                    KanjiOrder = kanjiForm.Order,
                    Reading = reading,
                    KanjiForm = kanjiForm,
                };
                reading.ReadingKanjiBridges ??= [];
                reading.ReadingKanjiBridges.Add(bridge);

                kanjiForm.ReadingKanjiBridges ??= [];
                kanjiForm.ReadingKanjiBridges.Add(bridge);
            }
        }
    }

    #endregion
}
