/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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
using Jitendex.JMdict.Entities.EntryElements.ReadingElements;

namespace Jitendex.JMdict.Entities.EntryElements;

[Table(nameof(Reading))]
[PrimaryKey(nameof(EntryId), nameof(Order))]
public class Reading
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public required string Text { get; set; }
    public required bool NoKanji { get; set; }

    public List<ReadingInfo> Infos { get; } = [];
    public List<ReadingPriority> Priorities { get; } = [];
    public List<Restriction> Restrictions { get; } = [];

    [ForeignKey(nameof(EntryId))]
    public required Entry Entry { get; set; }

    public bool IsHidden() => Infos.Any(static x => x.TagName == "sk");
}
