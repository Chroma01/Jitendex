using System.Xml;

namespace Jitendex.Warehouse.Jmdict.Models;

public class KanjiForm
{
    public required string Text { get; set; }
    public List<string>? InfoTags { get; set; }
    public const string XmlTagName = "k_ele";

    public async static Task<KanjiForm> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta)
    {
        var kanjiForm = new KanjiForm
        {
            Text = string.Empty
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
                    await ProcessTextAsync(reader, docMeta, currentTagName, kanjiForm);
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return kanjiForm;
    }

    private async static Task ProcessTextAsync(XmlReader reader, DocumentMetadata docMeta, string tagName, KanjiForm kanjiForm)
    {
        switch (tagName)
        {
            case "keb":
                kanjiForm.Text = await reader.GetValueAsync();
                break;
            case "ke_inf":
                var infoValue = await reader.GetValueAsync();
                var infoName = docMeta.EntityValueToName[infoValue];
                kanjiForm.InfoTags ??= [];
                kanjiForm.InfoTags.Add(infoName);
                break;
        }
    }
}
