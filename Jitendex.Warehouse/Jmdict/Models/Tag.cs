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

[PrimaryKey(nameof(EntryId), nameof(ReadingOrder), nameof(TagId))]
public class ReadingInfoTag
{
    public required int EntryId { get; set; }
    public required int ReadingOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(ReadingOrder)}")]
    public virtual Reading Reading { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual ReadingInfoTagDescription Description { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "re_inf";

    public async static Task<ReadingInfoTag> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, Reading reading)
    {
        var text = await reader.ReadAndGetTextValueAsync();
        var desc = docMeta.GetTagDescription<ReadingInfoTagDescription>(text);
        return new ReadingInfoTag
        {
            EntryId = reading.EntryId,
            ReadingOrder = reading.Order,
            TagId = desc.Id,
            Reading = reading,
            Description = desc,
        };
    }
    #endregion
}

[PrimaryKey(nameof(EntryId), nameof(ReadingOrder), nameof(TagId))]
public class ReadingPriorityTag
{
    public required int EntryId { get; set; }
    public required int ReadingOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(ReadingOrder)}")]
    public virtual Reading Reading { get; set; } = null!;
}

[PrimaryKey(nameof(EntryId), nameof(KanjiFormOrder), nameof(TagId))]
public class KanjiFormInfoTag
{
    public required int EntryId { get; set; }
    public required int KanjiFormOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(KanjiFormOrder)}")]
    public virtual KanjiForm KanjiForm { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual KanjiFormInfoTagDescription Description { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "ke_inf";

    public async static Task<KanjiFormInfoTag> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, KanjiForm kanjiForm)
    {
        var text = await reader.ReadAndGetTextValueAsync();
        var desc = docMeta.GetTagDescription<KanjiFormInfoTagDescription>(text);
        return new KanjiFormInfoTag
        {
            EntryId = kanjiForm.EntryId,
            KanjiFormOrder = kanjiForm.Order,
            TagId = desc.Id,
            KanjiForm = kanjiForm,
            Description = desc,
        };
    }
    #endregion
}

[PrimaryKey(nameof(EntryId), nameof(KanjiFormOrder), nameof(TagId))]
public class KanjiFormPriorityTag
{
    public required int EntryId { get; set; }
    public required int KanjiFormOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(KanjiFormOrder)}")]
    public virtual KanjiForm KanjiForm { get; set; } = null!;
}

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(TagId))]
public class PartOfSpeechTag
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual PartOfSpeechTagDescription Description { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "pos";

    public async static Task<PartOfSpeechTag> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, Sense sense)
    {
        var text = await reader.ReadAndGetTextValueAsync();
        var desc = docMeta.GetTagDescription<PartOfSpeechTagDescription>(text);
        return new PartOfSpeechTag
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            TagId = desc.Id,
            Sense = sense,
            Description = desc,
        };
    }
    #endregion
}

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(TagId))]
public class FieldTag
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual FieldTagDescription Description { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "field";

    public async static Task<FieldTag> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, Sense sense)
    {
        var text = await reader.ReadAndGetTextValueAsync();
        var desc = docMeta.GetTagDescription<FieldTagDescription>(text);
        return new FieldTag
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            TagId = desc.Id,
            Sense = sense,
            Description = desc,
        };
    }
    #endregion
}

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(TagId))]
public class MiscTag
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual MiscTagDescription Description { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "misc";

    public async static Task<MiscTag> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, Sense sense)
    {
        var text = await reader.ReadAndGetTextValueAsync();
        var desc = docMeta.GetTagDescription<MiscTagDescription>(text);
        return new MiscTag
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            TagId = desc.Id,
            Sense = sense,
            Description = desc,
        };
    }
    #endregion
}

[PrimaryKey(nameof(EntryId), nameof(SenseOrder), nameof(TagId))]
public class DialectTag
{
    public required int EntryId { get; set; }
    public required int SenseOrder { get; set; }
    public required string TagId { get; set; }

    [ForeignKey($"{nameof(EntryId)}, {nameof(SenseOrder)}")]
    public virtual Sense Sense { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual DialectTagDescription Description { get; set; } = null!;

    #region Static XML Factory

    public const string XmlTagName = "dial";

    public async static Task<DialectTag> FromXmlAsync(XmlReader reader, DocumentMetadata docMeta, Sense sense)
    {
        var text = await reader.ReadAndGetTextValueAsync();
        var desc = docMeta.GetTagDescription<DialectTagDescription>(text);
        return new DialectTag
        {
            EntryId = sense.EntryId,
            SenseOrder = sense.Order,
            TagId = desc.Id,
            Sense = sense,
            Description = desc,
        };
    }
    #endregion
}