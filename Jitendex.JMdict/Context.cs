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
using Jitendex.JMdict.Models;
using Jitendex.SQLite;

namespace Jitendex.JMdict;

public class Context : SqliteContext
{
    public DbSet<Entry> Entries { get; set; } = null!;
    public DbSet<Corpus> Corpora { get; set; } = null!;

    public DbSet<PriorityTag> PriorityTags { get; set; } = null!;
    public DbSet<ReadingInfoTag> ReadingInfoTags { get; set; } = null!;
    public DbSet<KanjiFormInfoTag> KanjiFormInfoTags { get; set; } = null!;

    public DbSet<PartOfSpeechTag> PartOfSpeechTags { get; set; } = null!;
    public DbSet<FieldTag> FieldTags { get; set; } = null!;
    public DbSet<MiscTag> MiscTags { get; set; } = null!;
    public DbSet<DialectTag> DialectTags { get; set; } = null!;

    public DbSet<GlossType> GlossTypes { get; set; } = null!;
    public DbSet<CrossReferenceType> CrossReferenceTypes { get; set; } = null!;
    public DbSet<LanguageSourceType> LanguageSourceTypes { get; set; } = null!;
    public DbSet<Language> Languages { get; set; } = null!;

    public DbSet<ExampleSourceType> ExampleSourceTypes { get; set; } = null!;
    public DbSet<ExampleSource> ExampleSources { get; set; } = null!;

    public Context() : base("jmdict.db") { }
}
