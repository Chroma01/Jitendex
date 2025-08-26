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
using Jitendex.Warehouse.Jmdict.Models.EntryElements.KanjiFormElements;

namespace Jitendex.Warehouse.Jmdict.Models.EntryElements;

[PrimaryKey(nameof(EntryId), nameof(Order))]
public class KanjiForm
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public virtual List<KInfo> Infos { get; set; } = [];
    public virtual List<KPriority> Priorities { get; set; } = [];
    public virtual List<ReadingKanjiFormBridge> ReadingBridges { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public virtual Entry Entry { get; set; } = null!;

    internal const string XmlTagName = "k_ele";

    public bool IsHidden() => Infos.Any(x => x.TagName == "sK");
}
