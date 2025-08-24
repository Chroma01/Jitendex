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
public class CrossReference
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required int Order { get; set; }

    public required string TypeName { get; set; }

    public required int RefEntryId { get; set; }
    public required int RefSenseOrder { get; set; }
    public required int RefReadingOrder { get; set; }
    public int? RefKanjiFormOrder { get; set; }

    [ForeignKey(nameof(TypeName))]
    public virtual CrossReferenceType Type { get; set; } = null!;

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey($"{nameof(RefEntryId)}, {nameof(RefSenseOrder)}")]
    public virtual Sense RefSense { get; set; } = null!;

    [ForeignKey($"{nameof(RefEntryId)}, {nameof(RefReadingOrder)}")]
    public virtual Reading RefReading { get; set; } = null!;

    [ForeignKey($"{nameof(RefEntryId)}, {nameof(RefKanjiFormOrder)}")]
    public virtual KanjiForm? RefKanjiForm { get; set; }

    [NotMapped]
    internal string RefText1 { get; set; } = null!;
    [NotMapped]
    internal string? RefText2 { get; set; }

    /// <summary>
    /// Stable and unique identifier for this reference in the raw data.
    /// </summary>
    internal string RawKey()
        => RefText2 is null ?
        $"{EntryId}・{Sense.Order}・{RefText1}・{RefSenseOrder}" :
        $"{EntryId}・{Sense.Order}・{RefText1}【{RefText2}】・{RefSenseOrder}";
}

internal static class CrossReferenceReader
{
    private record ParsedText(string Text1, string? Text2, int SenseOrder);

    public async static Task<CrossReference?> ReadCrossReferenceAsync(this XmlReader reader, Sense sense, EntityFactory factory)
    {
        var typeName = reader.Name;
        var text = await reader.ReadElementContentAsStringAsync();
        if (sense.Entry.CorpusId != CorpusId.Jmdict)
        {
            // TODO: Log
            return null;
        }
        ParsedText parsedText;
        try
        {
            parsedText = Parse(text);
        }
        catch
        {
            // TODO: Log
            return null;
        }
        var type = factory.GetKeywordByName<CrossReferenceType>(typeName);
        var crossRef = new CrossReference
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.CrossReferences.Count + 1,
            TypeName = typeName,
            Type = type,
            RefEntryId = -1,
            RefReadingOrder = -1,
            RefText1 = parsedText.Text1,
            RefText2 = parsedText.Text2,
            RefSenseOrder = parsedText.SenseOrder,
            Sense = sense,
        };
        return crossRef;
    }

    private static ParsedText Parse(string text)
    {
        const char separator = '・';
        var split = text.Split(separator);
        (string, string?, int) parsed;
        switch(split.Length)
        {
            case 1:
                parsed = (split[0], null, 1);
                break;
            case 2:
                if (int.TryParse(split[1], out int s1))
                    parsed = (split[0], null, s1);
                else
                    parsed = (split[0], split[1], 1);
                break;
            case 3:
                if (int.TryParse(split[2], out int s2))
                    parsed = (split[0], split[1], s2);
                else
                    throw new ArgumentException($"Third value in text `{text}` must be an integer", nameof(text));
                break;
            default:
                throw new ArgumentException($"Too many separator characters `{separator}` in text `{text}`", nameof(text));
        }
        return new ParsedText(parsed.Item1, parsed.Item2, parsed.Item3);
    }
}
