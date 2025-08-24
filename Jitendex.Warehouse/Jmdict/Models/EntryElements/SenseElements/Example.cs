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

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(Order))]
public class Example
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required int Order { get; set; }

    public required string SourceTypeName { get; set; }
    public required int SourceKey { get; set; }

    public required string Keyword { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey($"{nameof(SourceTypeName)}, {nameof(SourceKey)}")]
    public virtual ExampleSource Source { get; set; } = null!;

    internal const string XmlTagName = "example";
}


// Multiple <ex_sent> elements allowed, check for sanity.
internal static class ExampleReader
{
    public async static Task<Example> ReadExampleAsync(this XmlReader reader, Sense sense, KeywordFactory factory)
    {
        var example = new Example
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.Examples.Count + 1,
            SourceTypeName = string.Empty,
            SourceKey = -1,
            Keyword = string.Empty,
            Sense = sense,
        };

        var exit = false;
        while (!exit && await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    await reader.ReadChildElementAsync(example, factory);
                    break;
                case XmlNodeType.Text:
                    var text = await reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{Example.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = reader.Name == Example.XmlTagName;
                    break;
            }
        }

        return example;
    }

    private async static Task ReadChildElementAsync(this XmlReader reader, Example example, KeywordFactory factory)
    {
        switch (reader.Name)
        {
            case ExampleSource.XmlTagName:
                var sourceTypeName = reader.GetAttribute("exsrc_type");
                if (sourceTypeName is not null)
                {
                    example.SourceTypeName = sourceTypeName;
                }
                else
                {
                    // TODO: Log and warn.
                }
                var sourceText = await reader.ReadElementContentAsStringAsync();
                if (int.TryParse(sourceText, out int sourceKey))
                {
                    example.SourceKey = sourceKey;
                }
                else
                {
                    // TODO: Log and warn
                }
                example.Source = ExampleSource.FindByPrimaryKey(example.SourceTypeName, example.SourceKey, factory);
                break;
            case "ex_text":
                example.Keyword = await reader.ReadElementContentAsStringAsync();
                break;
            case "ex_sent":
                var sentenceLanguage = reader.GetAttribute("xml:lang");
                if (sentenceLanguage == "jpn")
                {
                    var text = await reader.ReadElementContentAsStringAsync();
                    if (example.Source.Text != string.Empty && example.Source.Text != text)
                    {
                        // TODO: Log and warn
                        Console.WriteLine($"Example source {example.SourceTypeName}:{example.SourceKey} has more than one text.");
                    }
                    example.Source.Text = text;
                }
                else if (sentenceLanguage == "eng")
                {
                    var translation = await reader.ReadElementContentAsStringAsync();
                    if (example.Source.Translation != string.Empty && example.Source.Translation != translation)
                    {
                        // TODO: Log and warn
                        Console.WriteLine($"Example source {example.SourceTypeName}:{example.SourceKey} has more than one translation.");
                    }
                    example.Source.Translation = translation;
                }
                else
                {
                    // TODO: Log and warn
                }
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{Example.XmlTagName}`");
        }
    }
}
