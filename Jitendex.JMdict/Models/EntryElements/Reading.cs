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
using Jitendex.JMdict.Models.EntryElements.ReadingElements;

namespace Jitendex.JMdict.Models.EntryElements;

[Table(nameof(Reading))]
[PrimaryKey(nameof(EntryId), nameof(Order))]
[Index(nameof(Text), IsUnique = false)]
public class Reading
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }

    public List<ReadingInfo> Infos { get; } = [];
    public List<ReadingPriority> Priorities { get; } = [];
    public List<KanjiForm> KanjiForms { get; } = [];

    [ForeignKey(nameof(EntryId))]
    public required Entry Entry { get; set; }

    [NotMapped]
    internal bool NoKanji { get; set; } = false;
    [NotMapped]
    internal List<Restriction> Restrictions { get; } = [];

    internal const string XmlTagName = "r_ele";
    internal const string Text_XmlTagName = "reb";
    internal const string NoKanji_XmlTagName = "re_nokanji";

    public bool IsHidden() => Infos.Any(static x => x.TagName == "sk");
}
