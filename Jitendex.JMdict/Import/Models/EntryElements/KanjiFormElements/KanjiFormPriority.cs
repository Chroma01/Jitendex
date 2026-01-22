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

namespace Jitendex.JMdict.Import.Models.EntryElements.KanjiFormElements;

internal sealed record KanjiFormPriority
{
    public required int EntryId { get; set; }
    public required int KanjiFormOrder { get; set; }
    public required int Order { get; set; }
    public required string TagName { get; set; }

    public (int, int, int) Key() => (EntryId, KanjiFormOrder, Order);

    public const string XmlTagName = "ke_pri";
}
