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
using Jitendex.Import.Kanjidic2.Models.EntryElements;

namespace Jitendex.Import.Kanjidic2.Models.Groups;

[NotMapped]
internal class ReadingMeaning
{
    public required string Character { get; set; }
    public List<Reading> Readings { get; set; } = [];
    public List<Meaning> Meanings { get; set; } = [];
    public bool IsKokuji = false;
    public bool IsGhost = false;

    [ForeignKey(nameof(Character))]
    public virtual Entry Entry { get; set; } = null!;

    internal const string XmlTagName = "rmgroup";
}
