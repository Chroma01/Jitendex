using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Kanjidic2.Models;

[Table("Kanjidic2.Readings")]
[PrimaryKey(nameof(Character), nameof(GroupOrder), nameof(Order))]
public class Reading
{
    public required string Character { get; set; }
    public required int GroupOrder { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public required string Type { get; set; }
    public const string XmlTagName = "reading";

    [ForeignKey($"{nameof(Character)}, {nameof(GroupOrder)}")]
    public virtual ReadingMeaningGroup Group { get; set; } = null!;

    public async static Task<Reading> FromXmlAsync(XmlReader reader, ReadingMeaningGroup group)
    {
        var reading = new Reading()
        {
            Character = group.Character,
            GroupOrder = group.Order,
            Order = (group.Readings?.Count ?? 0) + 1,
            Text = string.Empty,
            Type = reader.GetAttribute("r_type") ?? string.Empty,
            Group = group,
        };
        var exit = false;
        string currentTagName = XmlTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    break;
                case XmlNodeType.Text:
                    if (currentTagName == XmlTagName)
                    {
                        reading.Text = await reader.GetValueAsync();
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return reading;
    }
}
