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
using Jitendex.JMdict.Models.EntryElements;

namespace Jitendex.JMdict.Models;

[Table(nameof(Entry))]
public class Entry : ICorruptable
{
    public required int Id { get; set; }
    public required CorpusId CorpusId { get; set; }
    public List<Reading> Readings { get; set; } = [];
    public List<KanjiForm> KanjiForms { get; set; } = [];
    public List<Sense> Senses { get; set; } = [];

    [ForeignKey(nameof(CorpusId))]
    public required Corpus Corpus { get; set; }

    public bool IsCorrupt { get; set; }

    internal const string XmlTagName = "entry";
    internal const string Id_XmlTagName = "ent_seq";
}
