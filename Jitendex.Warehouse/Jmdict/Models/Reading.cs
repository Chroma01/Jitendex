using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.Warehouse.Jmdict.Models;

[Table("Jmdict.Readings")]
[PrimaryKey(nameof(EntryId), nameof(Order))]
public class Reading
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public required bool NoKanji { get; set; }
    public List<string>? InfoTags { get; set; }
    public List<string>? ConstraintKanjiFormTexts { get; set; }
    public const string XmlTagName = "r_ele";

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    public async static Task<Reading> FromXmlAsync(XmlReader reader, Entry entry, DocumentMetadata docMeta)
    {
        var reading = new Reading
        {
            EntryId = entry.Id,
            Order = entry.Readings.Count + 1,
            Text = string.Empty,
            NoKanji = false,
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
                    ProcessElement(currentTagName, reading);
                    break;
                case XmlNodeType.Text:
                    await ProcessTextAsync(reader, docMeta, currentTagName, reading);
                    break;
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }
        return reading;
    }

    private static void ProcessElement(string tagName, Reading reading)
    {
        switch (tagName)
        {
            case "re_nokanji":
                reading.NoKanji = true;
                break;
            default:
                // TODO: Log warning.
                break;
        }
    }

    private async static Task ProcessTextAsync(XmlReader reader, DocumentMetadata docMeta, string tagName, Reading reading)
    {
        switch (tagName)
        {
            case "reb":
                reading.Text = await reader.GetValueAsync();
                break;
            case "re_inf":
                var infoValue = await reader.GetValueAsync();
                var infoName = docMeta.EntityValueToName[infoValue];
                reading.InfoTags ??= [];
                reading.InfoTags.Add(infoName);
                break;
            case "re_restr":
                var kanjiFormText = await reader.GetValueAsync();
                reading.ConstraintKanjiFormTexts ??= [];
                reading.ConstraintKanjiFormTexts.Add(kanjiFormText);
                break;
            default:
                // TODO: Log warning.
                break;
        }
    }
}
