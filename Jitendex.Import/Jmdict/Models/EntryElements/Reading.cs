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
using Microsoft.EntityFrameworkCore;
using Jitendex.Import.Jmdict.Models.EntryElements.ReadingElements;

namespace Jitendex.Import.Jmdict.Models.EntryElements;

[PrimaryKey(nameof(EntryId), nameof(Order))]
public class Reading
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }

    public virtual List<ReadingInfo> Infos { get; set; } = [];
    public virtual List<ReadingPriority> Priorities { get; set; } = [];
    public virtual List<ReadingKanjiFormBridge> KanjiFormBridges { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    [NotMapped]
    internal bool NoKanji { get; set; } = false;
    [NotMapped]
    internal List<Restriction> Restrictions { get; set; } = [];

    internal const string XmlTagName = "r_ele";
    internal const string Text_XmlTagName = "reb";
    internal const string NoKanji_XmlTagName = "re_nokanji";

    public bool IsHidden() => Infos.Any(x => x.TagName == "sk");
}
