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

namespace Jitendex.Warehouse.Jmdict.Models;

[PrimaryKey(nameof(EntryId), nameof(Order))]
public class Sense
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public string? Note { get; set; }

    public List<PartOfSpeechTag>? PartOfSpeechTags { get; set; }
    public List<FieldTag>? FieldTags { get; set; }
    public List<MiscTag>? MiscTags { get; set; }
    public List<DialectTag>? DialectTags { get; set; }

    public List<Gloss>? Glosses { get; set; }

    // public List<LanguageSource>? LanguageSources { get; set; }
    // public List<CrossReference>? CrossReferences { get; set; }
    // public List<ExampleSentence>? ExampleSentences { get; set; }

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    [NotMapped]
    public List<string>? ReadingTextRestrictions { get; set; }
    [NotMapped]
    public List<string>? KanjiFormTextRestrictions { get; set; }

    #region Static XML Factory

    public const string XmlTagName = "sense";

    public async static Task<Sense> FromXmlAsync(XmlReader reader, Entry entry, DocumentMetadata docMeta)
    {
        var sense = new Sense
        {
            EntryId = entry.Id,
            Order = entry.Senses.Count + 1,
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
                    await ProcessElementAsync(reader, docMeta, sense);
                    break;
                case XmlNodeType.Text:
                    await ProcessTextAsync(reader, docMeta, sense, currentTagName);
                    break;
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
            case Gloss.XmlTagName:
                var gloss = await Gloss.FromXmlAsync(reader, sense, docMeta);
                if (gloss.Language == "eng")
                {
                    sense.Glosses ??= [];
                    sense.Glosses.Add(gloss);
                }
                break;
            case "xref":
                // TODO
                break;
            case "ant":
                // TODO
                break;
            case "example":
                // TODO
                break;
        }
    }

    private async static Task ProcessTextAsync(XmlReader reader, DocumentMetadata docMeta, Sense sense, string tagName)
    {
        var text = await reader.GetValueAsync();
        switch (tagName)
        {
            case "s_inf":
                if (sense.Note == null)
                {
                    sense.Note = text;
                }
                else
                {
                    // TODO: Properly log and warn
                    Console.WriteLine($"Jmdict entry {sense.EntryId} has more than one sense note.");
                }
                break;
            case "stagk":
                sense.KanjiFormTextRestrictions ??= [];
                sense.KanjiFormTextRestrictions.Add(text);
                break;
            case "stagr":
                sense.ReadingTextRestrictions ??= [];
                sense.ReadingTextRestrictions.Add(text);
                break;
            case "pos":
                var posTagDesc = docMeta.GetTagDescription<PartOfSpeechTagDescription>(text);
                var posTag = new PartOfSpeechTag
                {
                    EntryId = sense.EntryId,
                    SenseOrder = sense.Order,
                    TagId = posTagDesc.Id,
                    Sense = sense,
                    Description = posTagDesc,
                };
                sense.PartOfSpeechTags ??= [];
                sense.PartOfSpeechTags.Add(posTag);
                break;
            case "field":
                var fieldTagDesc = docMeta.GetTagDescription<FieldTagDescription>(text);
                var fieldTag = new FieldTag
                {
                    EntryId = sense.EntryId,
                    SenseOrder = sense.Order,
                    TagId = fieldTagDesc.Id,
                    Sense = sense,
                    Description = fieldTagDesc,
                };
                sense.FieldTags ??= [];
                sense.FieldTags.Add(fieldTag);
                break;
            case "misc":
                var miscTagDesc = docMeta.GetTagDescription<MiscTagDescription>(text);
                var miscTag = new MiscTag
                {
                    EntryId = sense.EntryId,
                    SenseOrder = sense.Order,
                    TagId = miscTagDesc.Id,
                    Sense = sense,
                    Description = miscTagDesc,
                };
                sense.MiscTags ??= [];
                sense.MiscTags.Add(miscTag);
                break;
            case "dial":
                var dialectTagDesc = docMeta.GetTagDescription<DialectTagDescription>(text);
                var dialectTag = new DialectTag
                {
                    EntryId = sense.EntryId,
                    SenseOrder = sense.Order,
                    TagId = dialectTagDesc.Id,
                    Sense = sense,
                    Description = dialectTagDesc,
                };
                sense.DialectTags ??= [];
                sense.DialectTags.Add(dialectTag);
                break;
        }
    }

    #endregion
}
