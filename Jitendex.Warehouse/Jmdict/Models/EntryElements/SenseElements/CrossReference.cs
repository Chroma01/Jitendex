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
    public required string Type { get; set; }

    public required int RefEntryId { get; set; }
    public required int RefSenseOrder { get; set; }
    public required int RefReadingOrder { get; set; }
    public int? RefKanjiFormOrder { get; set; }

    [NotMapped]
    internal string RefText1 { get; set; } = null!;
    [NotMapped]
    internal string? RefText2 { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    // [ForeignKey($"{nameof(RefEntryId)}, {nameof(RefSenseOrder)}")]
    // public virtual Sense RefSense { get; set; } = null!;

    // [ForeignKey($"{nameof(RefEntryId)}, {nameof(RefReadingOrder)}")]
    // public virtual Reading RefReading { get; set; } = null!;

    // [ForeignKey($"{nameof(RefEntryId)}, {nameof(RefKanjiFormOrder)}")]
    // public virtual KanjiForm? RefKanjiForm { get; set; }
}

internal static class CrossReferenceReader
{
    public async static Task<CrossReference> ReadCrossReferenceAsync(this XmlReader reader, Sense sense, DocumentMetadata docMeta)
    {
        var type = reader.Name;
        var text = await reader.ReadElementContentAsStringAsync();
        var parsedText = ParseCrossReference(text);
        var crossRef = new CrossReference
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            Order = sense.CrossReferences.Count + 1,
            Type = type,
            RefEntryId = -1,
            RefReadingOrder = -1,
            RefText1 = parsedText.Item1,
            RefText2 = parsedText.Item2,
            RefSenseOrder = parsedText.Item3,
            Sense = sense,
        };
        return crossRef;
    }

    private static (string, string?, int) ParseCrossReference(string text)
    {
        var split = text.Split('・');
        switch(split.Length)
        {
            case 1:
                return (split[0], null, 1);
            case 2:
                if (int.TryParse(split[1], out int number))
                    return (split[0], null, number);
                else
                    return (split[0], split[1], 1);
            case 3:
                return (split[0], split[1], int.Parse(split[2]));
            default:
                throw new Exception($"Unable to parse cross-reference text: `{text}`");
        }
    }
}
