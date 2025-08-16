using System.Xml;

namespace Jitendex.Warehouse.Kanjidic2.Models;

public class Reading
{
    public required string Text { get; set; }
    public required string Type { get; set; }
    public const string XmlTagName = "reading";

    public async static Task<Reading> FromXmlAsync(XmlReader reader)
    {
        var reading = new Reading()
        {
            Text = string.Empty,
            Type = reader.GetAttribute("r_type") ?? string.Empty,
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
