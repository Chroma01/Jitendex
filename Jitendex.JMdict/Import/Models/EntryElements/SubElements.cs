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

namespace Jitendex.JMdict.Import.Models.EntryElements;

internal abstract record SubElement(int EntryId, int ParentOrder, int Order)
{
    public (int, int, int) Key() => (EntryId, ParentOrder, Order);
}

internal sealed record KanjiFormInfoElement
    (int EntryId, int ParentOrder, int Order, string TagName)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record KanjiFormPriorityElement
    (int EntryId, int ParentOrder, int Order, string TagName)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record ReadingInfoElement
    (int EntryId, int ParentOrder, int Order, string TagName)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record ReadingPriorityElement
    (int EntryId, int ParentOrder, int Order, string TagName)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record RestrictionElement
    (int EntryId, int ParentOrder, int Order, string KanjiFormText)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record CrossReferenceElement
    (int EntryId, int ParentOrder, int Order, string TypeName, string RefText1, string? RefText2, int RefSenseNumber)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record DialectElement
    (int EntryId, int ParentOrder, int Order, string TagName)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record FieldElement
    (int EntryId, int ParentOrder, int Order, string TagName)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record GlossElement
    (int EntryId, int ParentOrder, int Order, string TypeName, string Text)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record KanjiFormRestrictionElement
    (int EntryId, int ParentOrder, int Order, string KanjiFormText)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record LanguageSourceElement
    (int EntryId, int ParentOrder, int Order, string? Text, string LanguageCode, string TypeName, bool IsWasei)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record MiscElement
    (int EntryId, int ParentOrder, int Order, string TagName)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record PartOfSpeechElement
    (int EntryId, int ParentOrder, int Order, string TagName)
    : SubElement(EntryId, ParentOrder, Order);

internal sealed record ReadingRestrictionElement
    (int EntryId, int ParentOrder, int Order, string ReadingText)
    : SubElement(EntryId, ParentOrder, Order);
