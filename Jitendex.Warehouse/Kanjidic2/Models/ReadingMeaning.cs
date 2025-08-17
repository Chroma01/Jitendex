using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Jitendex.Warehouse.Kanjidic2.Models;

[Table("Kanjidic2.ReadingMeanings")]
public class ReadingMeaning
{
    [Key]
    public required string Character { get; set; }
    public List<ReadingMeaningGroup>? Groups { get; set; }
    public List<string>? Nanori { get; set; }
    public const string XmlTagName = "reading_meaning";

    [ForeignKey(nameof(Character))]
    public virtual Entry Entry { get; set; } = null!;

    public async static Task<ReadingMeaning> FromXmlAsync(XmlReader reader, Entry entry)
    {
        var readingMeaning = new ReadingMeaning
        {
            Character = entry.Character,
            Entry = entry,
        };
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    if (currentTagName == "rmgroup")
                    {
                        var group = await ReadingMeaningGroup.FromXmlAsync(reader, readingMeaning);
                        readingMeaning.Groups ??= [];
                        readingMeaning.Groups.Add(group);
                    }
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == "nanori")
                    {
                        var text = await reader.GetValueAsync();
                        readingMeaning.Nanori ??= [];
                        readingMeaning.Nanori.Add(text);
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return readingMeaning;
    }
}
