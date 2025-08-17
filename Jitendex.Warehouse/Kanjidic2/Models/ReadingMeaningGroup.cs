using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Kanjidic2.Models;

[Table("Kanjidic2.ReadingMeaningGroups")]
[PrimaryKey(nameof(Character), nameof(Order))]
public class ReadingMeaningGroup
{
    public required string Character { get; set; }
    public required int Order { get; set; }
    public List<Reading>? Readings { get; set; }
    public List<Meaning>? Meanings { get; set; }
    public const string XmlTagName = "rmgroup";

    [ForeignKey(nameof(Character))]
    public virtual ReadingMeaning ReadingMeaning { get; set; } = null!;

    public async static Task<ReadingMeaningGroup> FromXmlAsync(XmlReader reader, ReadingMeaning readingMeaning)
    {
        var group = new ReadingMeaningGroup
        {
            Character = readingMeaning.Character,
            Order = (readingMeaning.Groups?.Count ?? 0) + 1,
            ReadingMeaning = readingMeaning,
        };
        var exit = false;
        string currentTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    await ProcessElementAsync(reader, currentTagName, group);
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return group;
    }

    private async static Task ProcessElementAsync(XmlReader reader, string tagName, ReadingMeaningGroup group)
    {
        switch (tagName)
        {
            case Reading.XmlTagName:
                var reading = await Reading.FromXmlAsync(reader, group);
                group.Readings ??= [];
                group.Readings.Add(reading);
                break;
            case Meaning.XmlTagName:
                var meaning = await Meaning.FromXmlAsync(reader, group);
                group.Meanings ??= [];
                group.Meanings.Add(meaning);
                break;
        }
    }
}
