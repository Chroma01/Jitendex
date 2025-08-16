using System.Xml;

namespace Jitendex.Warehouse.Jmdict.Models;

public class Entry
{
    public required int Sequence { get; set; }
    public required List<Reading> Readings { get; set; }
    public List<KanjiForm>? KanjiForms { get; set; }
    public const string XmlTagName = "entry";

    public async static Task<Entry> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta)
    {
        var entry = new Entry
        {
            Sequence = -1,
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
                    entry.Sequence = sequence;
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
}
