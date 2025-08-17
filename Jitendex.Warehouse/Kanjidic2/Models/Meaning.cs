using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Kanjidic2.Models;

[Table("Kanjidic2.Meanings")]
[PrimaryKey(nameof(Character), nameof(GroupOrder), nameof(Order))]
public class Meaning
{
    public required string Character { get; set; }
    public required int GroupOrder { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public required string Language { get; set; }
    public const string XmlTagName = "meaning";

    [ForeignKey($"{nameof(Character)}, {nameof(GroupOrder)}")]
    public virtual ReadingMeaningGroup Group { get; set; } = null!;

    public async static Task<Meaning> FromXmlAsync(XmlReader reader, ReadingMeaningGroup group)
    {
        var meaning = new Meaning
        {
            Character = group.Character,
            GroupOrder = group.Order,
            Order = (group.Meanings?.Count ?? 0) + 1,
            Text = string.Empty,
            Language = reader.GetAttribute("m_lang") ?? "en",
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
                        meaning.Text = await reader.GetValueAsync();
                    }
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return meaning;
    }
}
