/*
Copyright (c) 2025 Stephen Kraus

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the
terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along
with Jitendex. If not, see <https://www.gnu.org/licenses/>.
*/

using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements;

[PrimaryKey(nameof(EntryId), nameof(Order))]
public class Sense
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public string? Note { get; set; }

    public List<PartOfSpeechTag> PartOfSpeechTags { get; set; } = [];
    public List<FieldTag> FieldTags { get; set; } = [];
    public List<MiscTag> MiscTags { get; set; } = [];
    public List<DialectTag> DialectTags { get; set; } = [];

    public List<Gloss> Glosses { get; set; } = [];

    // public List<LanguageSource> LanguageSources { get; set; } = [];
    // public List<CrossReference> CrossReferences { get; set; } = [];
    // public List<ExampleSentence> ExampleSentences { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    // TODO: Derive these [NotMapped] properties from mapped properties.
    [NotMapped]
    public List<string> ReadingTextRestrictions { get; set; } = [];
    [NotMapped]
    public List<string> KanjiFormTextRestrictions { get; set; } = [];
    [NotMapped]
    public List<string> Xrefs { get; set; } = [];
    [NotMapped]
    public List<string> Ants { get; set; } = [];
    [NotMapped]
    public List<string> Lsources { get; set; } = [];

    #region Static XML Factory

    public const string XmlTagName = "sense";

    public async static Task<Sense> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, Entry entry)
    {
        var sense = new Sense
        {
            EntryId = entry.Id,
            Order = entry.Senses.Count + 1,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    await ProcessElementAsync(reader, docMeta, sense);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == XmlTagName;
                    break;
            }
        }

        return sense;
    }

    private async static Task ProcessElementAsync(XmlReader reader, DocumentMetadata docMeta, Sense sense)
    {
        switch (reader.Name)
        {
            case "stagk":
                var kanjiFormTextRestriction = await reader.ReadAndGetTextValueAsync();
                sense.KanjiFormTextRestrictions.Add(kanjiFormTextRestriction);
                break;
            case "stagr":
                var readingTextRestriction = await reader.ReadAndGetTextValueAsync();
                sense.ReadingTextRestrictions.Add(readingTextRestriction);
                break;
            case "s_inf":
                // The XML schema allows for more than one note per sense,
                // but in practice there is only one or none.
                if (sense.Note != null)
                    throw new Exception($"Jmdict entry {sense.EntryId} has more than one sense note.");
                sense.Note = await reader.ReadAndGetTextValueAsync();
                break;
            case Gloss.XmlTagName:
                var gloss = await Gloss.FromXmlAsync(reader, sense);
                if (gloss.Language == "eng")
                {
                    sense.Glosses.Add(gloss);
                }
                break;
            case PartOfSpeechTag.XmlTagName:
                var posTag = await PartOfSpeechTag.FromXmlAsync(reader, docMeta, sense);
                sense.PartOfSpeechTags.Add(posTag);
                break;
            case FieldTag.XmlTagName:
                var fieldTag = await FieldTag.FromXmlAsync(reader, docMeta, sense);
                sense.FieldTags.Add(fieldTag);
                break;
            case MiscTag.XmlTagName:
                var miscTag = await MiscTag.FromXmlAsync(reader, docMeta, sense);
                sense.MiscTags.Add(miscTag);
                break;
            case DialectTag.XmlTagName:
                var dialTag = await DialectTag.FromXmlAsync(reader, docMeta, sense);
                sense.DialectTags.Add(dialTag);
                break;
            case "xref":
                var xref = await reader.ReadAndGetTextValueAsync();
                sense.Xrefs.Add(xref);
                break;
            case "ant":
                var ant = await reader.ReadAndGetTextValueAsync();
                sense.Xrefs.Add(ant);
                break;
            case "lsource":
                if (!reader.IsEmptyElement)
                {
                    var lsource = await reader.ReadAndGetTextValueAsync();
                    sense.Lsources.Add(lsource);
                }
                break;
            case "example":
                // TODO
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{XmlTagName}`");
        }
    }

    #endregion
}
