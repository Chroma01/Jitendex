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

using System.Xml;
using Jitendex.Warehouse.Jmdict.Models;
using Jitendex.Warehouse.Jmdict.Models.EntryElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.SenseElements;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

internal class ExampleReader
{
    private readonly XmlReader Reader;
    private readonly EntityFactory Factory;

    public ExampleReader(XmlReader reader, EntityFactory factory)
    {
        Reader = reader;
        Factory = factory;
    }

    public async Task<Example> ReadAsync(Sense sense)
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
        while (!exit && await Reader.ReadAsync())
        {
            switch (Reader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(example);
                    break;
                case XmlNodeType.Text:
                    var text = await Reader.GetValueAsync();
                    throw new Exception($"Unexpected text node found in `{Example.XmlTagName}`: `{text}`");
                case XmlNodeType.EndElement:
                    exit = Reader.Name == Example.XmlTagName;
                    break;
            }
        }

        return example;
    }

    private async Task ReadChildElementAsync(Example example)
    {
        switch (Reader.Name)
        {
            case ExampleSource.XmlTagName:
                var sourceTypeName = Reader.GetAttribute("exsrc_type");
                if (sourceTypeName is not null)
                {
                    example.SourceTypeName = sourceTypeName;
                }
                else
                {
                    // TODO: Log and warn.
                }
                var sourceText = await Reader.ReadElementContentAsStringAsync();
                if (int.TryParse(sourceText, out int sourceKey))
                {
                    example.SourceKey = sourceKey;
                }
                else
                {
                    // TODO: Log and warn
                }
                example.Source = Factory.GetExampleSource(example.SourceTypeName, example.SourceKey);
                break;
            case "ex_text":
                example.Keyword = await Reader.ReadElementContentAsStringAsync();
                break;
            case "ex_sent":
                var sentenceLanguage = Reader.GetAttribute("xml:lang");
                if (sentenceLanguage == "jpn")
                {
                    var text = await Reader.ReadElementContentAsStringAsync();
                    if (example.Source.Text != string.Empty && example.Source.Text != text)
                    {
                        // TODO: Log and warn
                        Console.WriteLine($"Example source {example.SourceTypeName}:{example.SourceKey} has more than one text.");
                    }
                    example.Source.Text = text;
                }
                else if (sentenceLanguage == "eng")
                {
                    var translation = await Reader.ReadElementContentAsStringAsync();
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
                throw new Exception($"Unexpected XML element node named `{Reader.Name}` found in element `{Example.XmlTagName}`");
        }
    }
}
