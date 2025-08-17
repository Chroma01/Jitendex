using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Jitendex.Warehouse.Kanjidic2.Models;

[Table("Kanjidic2.Entries")]
public class Entry
{
    [Key]
    public required string Character { get; set; }
    public ReadingMeaning? ReadingMeaning { get; set; }

    public const string XmlTagName = "character";

    public async static Task<Entry> FromXmlAsync(XmlReader reader)
    {
        var entry = new Entry
        {
            Character = string.Empty,
        };
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    if (currentTagName == ReadingMeaning.XmlTagName)
                    {
                        entry.ReadingMeaning = await ReadingMeaning.FromXmlAsync(reader, entry);
                    }
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == "literal")
                    {
                        entry.Character = await reader.GetValueAsync();
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return entry;
    }
}