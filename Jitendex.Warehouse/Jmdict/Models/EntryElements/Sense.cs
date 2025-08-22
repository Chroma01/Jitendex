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

    public virtual List<PartOfSpeech> PartsOfSpeech { get; set; } = [];
    public virtual List<Field> Fields { get; set; } = [];
    public virtual List<Misc> Miscs { get; set; } = [];
    public virtual List<Dialect> Dialects { get; set; } = [];

    public virtual List<Gloss> Glosses { get; set; } = [];

    // public List<LanguageSource> LanguageSources { get; set; } = [];
    // public List<ExampleSentence> ExampleSentences { get; set; } = [];

    [InverseProperty(nameof(CrossReference.Sense))]
    public virtual List<CrossReference> CrossReferences { get; set; } = [];

    [InverseProperty(nameof(CrossReference.RefSense))]
    public virtual List<CrossReference> ReverseCrossReferences { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    [NotMapped]
    internal List<string> ReadingTextRestrictions { get; set; } = [];
    [NotMapped]
    internal List<string> KanjiFormTextRestrictions { get; set; } = [];
    [NotMapped]
    internal List<string> Lsources { get; set; } = [];

    internal const string XmlTagName = "sense";
}

internal static class SenseReader
{
    public async static Task<Sense> ReadSenseAsync(this XmlReader reader, Entry entry, DocumentMetadata docMeta)
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
                    await reader.ReadChildElementAsync(sense, docMeta);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{Sense.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == Sense.XmlTagName;
                    break;
            }
        }

        return sense;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, Sense sense, DocumentMetadata docMeta)
    {
        switch (reader.Name)
        {
            case "stagk":
                var kanjiFormTextRestriction = await reader.ReadElementContentAsStringAsync();
                sense.KanjiFormTextRestrictions.Add(kanjiFormTextRestriction);
                break;
            case "stagr":
                var readingTextRestriction = await reader.ReadElementContentAsStringAsync();
                sense.ReadingTextRestrictions.Add(readingTextRestriction);
                break;
            case "s_inf":
                if (sense.Note != null)
                {
                    // The XML schema allows for more than one note per sense,
                    // but in practice there is only one or none.
                    // TODO: Log warning
                }
                sense.Note = await reader.ReadElementContentAsStringAsync();
                break;
            case Gloss.XmlTagName:
                var gloss = await reader.ReadGlossAsync(sense);
                if (gloss.Language == "eng")
                {
                    sense.Glosses.Add(gloss);
                }
                break;
            case PartOfSpeech.XmlTagName:
                var pos = await reader.ReadPartOfSpeechAsync(sense, docMeta);
                sense.PartsOfSpeech.Add(pos);
                break;
            case Field.XmlTagName:
                var field = await reader.ReadFieldAsync(sense, docMeta);
                sense.Fields.Add(field);
                break;
            case Misc.XmlTagName:
                var misc = await reader.ReadMiscAsync(sense, docMeta);
                sense.Miscs.Add(misc);
                break;
            case Dialect.XmlTagName:
                var dial = await reader.ReadDialectAsync(sense, docMeta);
                sense.Dialects.Add(dial);
                break;
            case "xref":
            case "ant":
                var reference = await reader.ReadCrossReferenceAsync(sense);
                if (reference is not null)
                {
                    sense.CrossReferences.Add(reference);
                }
                break;
            case "lsource":
                if (!reader.IsEmptyElement)
                {
                    var lsource = await reader.ReadElementContentAsStringAsync();
                    sense.Lsources.Add(lsource);
                }
                break;
            case "example":
                // TODO
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{Sense.XmlTagName}`");
        }
    }
}
