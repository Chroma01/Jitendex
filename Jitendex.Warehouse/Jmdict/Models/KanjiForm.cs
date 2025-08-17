using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Jmdict.Models;

[Table("Jmdict.KanjiForms")]
[PrimaryKey(nameof(EntryId), nameof(Order))]
public class KanjiForm
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public List<string>? InfoTags { get; set; }
    public const string XmlTagName = "k_ele";

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    public async static Task<KanjiForm> FromXmlAsync(XmlReader reader, Entry entry, DocumentMetadata docMeta)
    {
        var kanjiForm = new KanjiForm
        {
            EntryId = entry.Id,
            Order = (entry.KanjiForms?.Count ?? 0) + 1,
            Text = string.Empty,
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
            default:
                // TODO: Log warning.
                break;
        }
    }
}
