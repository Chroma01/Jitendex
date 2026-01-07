/*
Copyright (c) 2025 Stephen Kraus
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

using Microsoft.EntityFrameworkCore;
using Jitendex.SQLite;
using Jitendex.Tatoeba.Entities;

namespace Jitendex.Tatoeba;

public sealed class Context : SqliteContext
{
    public DbSet<DocumentMetadata> Metadata { get; set; } = null!;
    public DbSet<Sequence> Sequences { get; set; } = null!;
    public DbSet<EnglishSequence> EnglishSequences { get; set; } = null!;
    public DbSet<JapaneseSequence> JapaneseSequences { get; set; } = null!;
    public DbSet<TokenizedSentence> TokenizedSentences { get; set; } = null!;
    public DbSet<Token> Tokens { get; set; } = null!;
    public DbSet<Revision> Revisions { get; set; } = null!;
    public Context() : base("tatoeba.db") { }
}
