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

using Microsoft.EntityFrameworkCore;
using Jitendex.Chise.Models;
using Jitendex.SQLite;
using Jitendex.Chise.Models.Sequences;

namespace Jitendex.Chise;

public class Context : SqliteContext
{
    public DbSet<Codepoint> Codepoints { get; } = null!;
    public DbSet<Component> Components { get; } = null!;
    public DbSet<ComponentPosition> ComponentPositions { get; } = null!;
    public DbSet<UnicodeCharacter> UnicodeCharacters { get; } = null!;

    public DbSet<AboveToBelowSequence> AboveToBelowSequence { get; } = null!;
    public DbSet<AboveToMiddleAndBelowSequence> AboveToMiddleAndBelowSequences { get; } = null!;
    public DbSet<FullSurroundSequence> FullSurroundSequences { get; } = null!;
    public DbSet<LeftToMiddleAndRightSequence> LeftToMiddleAndRightSequences { get; } = null!;
    public DbSet<LeftToRightSequence> LeftToRightSequences { get; } = null!;
    public DbSet<OverlaidSequence> OverlaidSequences { get; } = null!;
    public DbSet<SurroundFromAboveSequence> SurroundFromAboveSequences { get; } = null!;
    public DbSet<SurroundFromBelowSequence> SurroundFromBelowSequences { get; } = null!;
    public DbSet<SurroundFromLeftAndRightSequence> SurroundFromLeftAndRightSequences { get; } = null!;
    public DbSet<SurroundFromLeftSequence> SurroundFromLeftSequences { get; } = null!;
    public DbSet<SurroundFromLowerLeftAndRightSequence> SurroundFromLowerLeftAndRightSequences { get; } = null!;
    public DbSet<SurroundFromLowerLeftSequence> SurroundFromLowerLeftSequences { get; } = null!;
    public DbSet<SurroundFromLowerRightSequence> SurroundFromLowerRightSequences { get; } = null!;
    public DbSet<SurroundFromRightSequence> SurroundFromRightSequences { get; } = null!;
    public DbSet<SurroundFromUpperLeftAndRightSequence> SurroundFromUpperLeftAndRightSequences { get; } = null!;
    public DbSet<SurroundFromUpperLeftSequence> SurroundFromUpperLeftSequences { get; } = null!;
    public DbSet<SurroundFromUpperRightSequence> SurroundFromUpperRightSequences { get; } = null!;

    public Context() : base("chise_ids.db") { }
}
