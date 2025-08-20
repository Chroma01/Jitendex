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

using System.ComponentModel.DataAnnotations;
using System.Xml;
using Jitendex.Warehouse.Kanjidic2.Models.Groups;
using Jitendex.Warehouse.Kanjidic2.Models.EntryElements;

namespace Jitendex.Warehouse.Kanjidic2.Models;

public class Entry
{
    [Key]
    public required string Character { get; set; }
    public List<Codepoint> Codepoints { get; set; } = [];
    public List<Radical> Radicals { get; set; } = [];
    public List<Reading> Readings { get; set; } = [];
    public List<Meaning> Meanings { get; set; } = [];
    public List<Nanori> Nanoris { get; set; } = [];
    public int? Grade { get; set; }
    public int? Frequency { get; set; }
    public int? JlptLevel { get; set; }
    public List<StrokeCount> StrokeCounts { get; set; } = [];
    public List<Variant> Variants { get; set; } = [];
    public List<RadicalName> RadicalNames { get; set; } = [];
    public List<Dictionary> Dictionaries { get; set; } = [];

    internal const string XmlTagName = "character";
}

internal static class EntryReader
{
    public async static Task<Entry> ReadElementContentAsEntryAsync(this XmlReader reader)
    {
        var entry = new Entry
        {
            Character = string.Empty,
        };

        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    await reader.ReadChildElementAsync(entry);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{Entry.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == Entry.XmlTagName;
                    break;
            }
        }
        return entry;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, Entry entry)
    {
        switch (reader.Name)
        {
            case "literal":
                entry.Character = await reader.ReadElementContentAsStringAsync();
                break;
            case CodepointGroup.XmlTagName:
                if (entry.Codepoints.Count != 0)
                    throw new Exception($"Character {entry.Character} has more than one codepoint group.");
                var codepointGroup = await reader.ReadElementContentAsCodepointGroupAsync(entry);
                entry.Codepoints = codepointGroup.Codepoints;
                break;
            case RadicalGroup.XmlTagName:
                if (entry.Radicals.Count != 0)
                    throw new Exception($"Character {entry.Character} has more than one radical group.");
                var radicalGroup = await reader.ReadElementContentAsRadicalGroupAsync(entry);
                entry.Radicals = radicalGroup.Radicals;
                break;
            case ReadingMeaningGroup.XmlTagName:
                if (entry.Readings.Count != 0 || entry.Meanings.Count != 0 || entry.Nanoris.Count != 0)
                    throw new Exception($"Character {entry.Character} has more than one reading/meaning group.");
                var readingMeaningGroup = await reader.ReadElementContentAsReadingMeaningGroupAsync(entry);
                entry.Readings = readingMeaningGroup.ReadingMeaning?.Readings ?? [];
                entry.Meanings = readingMeaningGroup.ReadingMeaning?.Meanings ?? [];
                entry.Nanoris = readingMeaningGroup.Nanoris;
                break;
            case MiscGroup.XmlTagName:
                if (entry.Grade != null || entry.Frequency != null || entry.JlptLevel != null)
                    throw new Exception($"Character {entry.Character} has more than one misc group.");
                if (entry.StrokeCounts.Count != 0 || entry.Variants.Count != 0 || entry.RadicalNames.Count != 0)
                    throw new Exception($"Character {entry.Character} has more than one misc group.");
                var miscGroup = await reader.ReadElementContentAsMiscGroupAsync(entry);
                entry.Grade = miscGroup.Grade;
                entry.Frequency = miscGroup.Frequency;
                entry.JlptLevel = miscGroup.JlptLevel;
                entry.StrokeCounts = miscGroup.StrokeCounts;
                entry.Variants = miscGroup.Variants;
                entry.RadicalNames = miscGroup.RadicalNames;
                break;
            case DictionaryGroup.XmlTagName:
                if (entry.Dictionaries.Count != 0)
                    throw new Exception($"Character {entry.Character} has more than one dictionary group.");
                var dictionaryGroup = await reader.ReadElementContentAsDictionaryGroupAsync(entry);
                entry.Dictionaries = dictionaryGroup.Dictionaries;
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{Entry.XmlTagName}`");
        }
    }
}