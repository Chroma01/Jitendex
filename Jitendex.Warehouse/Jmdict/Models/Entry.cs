using System.Xml;

namespace Jitendex.Warehouse.Jmdict.Models;

public class Entry
{
    public required List<Reading> Readings { get; set; }
    public List<KanjiForm>? KanjiForms { get; set; }
    public const string XmlTagName = "entry";

    public async static Task<Entry> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta)
    {
        var entry = new Entry
        {
            Readings = [],
        };
        var exit = false;
        string currentTagName;

        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentTagName = reader.Name;
                    await ProcessElementAsync(reader, docMeta, currentTagName, entry);
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return entry;
    }

    private async static Task ProcessElementAsync(XmlReader reader, DocumentMetadata docMeta, string tagName, Entry entry)
    {
        switch (tagName)
        {
            case KanjiForm.XmlTagName:
                var kanjiForm = await KanjiForm.FromXmlAsync(reader, docMeta);
                entry.KanjiForms ??= [];
                entry.KanjiForms.Add(kanjiForm);
                break;
            case Reading.XmlTagName:
                var reading = await Reading.FromXmlAsync(reader, docMeta);
                entry.Readings.Add(reading);
                break;
        }
    }
}
