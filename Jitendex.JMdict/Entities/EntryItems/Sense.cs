/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Jitendex.JMdict.Entities.EntryItems.SenseItems;

namespace Jitendex.JMdict.Entities.EntryItems;

[Table(nameof(Sense))]
[PrimaryKey(nameof(EntryId), nameof(Order))]
public sealed class Sense
{
    public required int EntryId { get; set; }
    public required int Order { get; set; }
    public string? Note { get; set; }

    public List<KanjiFormRestriction> KanjiFormRestrictions { get; set; } = [];
    public List<ReadingRestriction> ReadingRestrictions { get; set; } = [];

    public List<PartOfSpeech> PartsOfSpeech { get; set; } = [];
    public List<Field> Fields { get; set; } = [];
    public List<Misc> Miscs { get; set; } = [];
    public List<Dialect> Dialects { get; set; } = [];

    public List<Gloss> Glosses { get; set; } = [];
    public List<LanguageSource> LanguageSources { get; set; } = [];
    public List<CrossReference> CrossReferences { get; set; } = [];

    [ForeignKey(nameof(EntryId))]
    public Entry Entry { get; set; } = null!;

    public (int, int) Key() => (EntryId, Order);
}
