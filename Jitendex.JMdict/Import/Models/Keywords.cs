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

namespace Jitendex.JMdict.Import.Models;

internal interface IKeyword
{
    string Name { get; init; }
    DateOnly CreatedDate { get; init; }
}

internal sealed record ReadingInfoTag : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record KanjiFormInfoTag : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record PartOfSpeechTag : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record FieldTag : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record MiscTag : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record DialectTag : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record GlossType : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record CrossReferenceType : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record LanguageSourceType : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record PriorityTag : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}

internal sealed record Language : IKeyword
{
    public required string Name { get; init; }
    public required DateOnly CreatedDate { get; init; }
}
