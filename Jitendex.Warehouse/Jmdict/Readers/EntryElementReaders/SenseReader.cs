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
using Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders.SenseElementReaders;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders;

internal static class SenseReader
{
    public async static Task<Sense> ReadSenseAsync(this XmlReader reader, Entry entry, EntityFactory factory)
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
                    await reader.ReadChildElementAsync(sense, factory);
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

    private async static Task ReadChildElementAsync(this XmlReader reader, Sense sense, EntityFactory factory)
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
                var gloss = await reader.ReadGlossAsync(sense, factory);
                if (gloss.Language == "eng")
                {
                    sense.Glosses.Add(gloss);
                }
                break;
            case PartOfSpeech.XmlTagName:
                var pos = await reader.ReadPartOfSpeechAsync(sense, factory);
                sense.PartsOfSpeech.Add(pos);
                break;
            case Field.XmlTagName:
                var field = await reader.ReadFieldAsync(sense, factory);
                sense.Fields.Add(field);
                break;
            case Misc.XmlTagName:
                var misc = await reader.ReadMiscAsync(sense, factory);
                sense.Miscs.Add(misc);
                break;
            case Dialect.XmlTagName:
                var dial = await reader.ReadDialectAsync(sense, factory);
                sense.Dialects.Add(dial);
                break;
            case "xref":
            case "ant":
                var reference = await reader.ReadCrossReferenceAsync(sense, factory);
                if (reference is not null)
                {
                    sense.CrossReferences.Add(reference);
                }
                break;
            case LanguageSource.XmlTagName:
                var languageSource = await reader.ReadLanguageSourceAsync(sense, factory);
                sense.LanguageSources.Add(languageSource);
                break;
            case Example.XmlTagName:
                var example = await reader.ReadExampleAsync(sense, factory);
                sense.Examples.Add(example);
                break;
            default:
                throw new Exception($"Unexpected XML element node named `{reader.Name}` found in element `{Sense.XmlTagName}`");
        }
    }
}
