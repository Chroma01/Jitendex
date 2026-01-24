/*
Copyright (c) 2026 Stephen Kraus
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

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Jitendex.Dto.JMdict;
using Jitendex.JMdict.Entities.EntryElements;

namespace Jitendex.JMdict;

public static class DtoMapper
{
    public static Dictionary<int, SequenceDto> LoadSequencesWithoutRevisions(Context context, IReadOnlySet<int> sequenceIds)
        => context.Sequences
            .AsNoTracking()
            .AsSplitQuery()
            .Where(s => sequenceIds.Contains(s.Id))
            .OrderBy(static s => s.Id)
            .Select(static seq => new SequenceDto(seq.Id, seq.CreatedDate)
            {
                Entry = seq.Entry == null ? null : new EntryDto
                {
                    KanjiForms = seq.Entry.KanjiForms
                        .AsQueryable()
                        .OrderBy(static k => k.Order)
                        .Select(KanjiFormProjection)
                        .ToList(),
                    Readings = seq.Entry.Readings
                        .AsQueryable()
                        .OrderBy(static r => r.Order)
                        .Select(ReadingProjection)
                        .ToList(),
                    Senses = seq.Entry.Senses
                        .AsQueryable()
                        .OrderBy(static s => s.Order)
                        .Select(SenseProjection)
                        .ToList()
                }
            })
            .ToDictionary(static s => s.Id);

    private static readonly Expression<Func<KanjiForm, KanjiFormDto>> KanjiFormProjection =
        static k => new KanjiFormDto(k.Text)
        {
            Infos = k.Infos
                .OrderBy(static x => x.Order)
                .Select(static x => new KanjiFormInfoDto(x.TagName))
                .ToList(),
            Priorities = k.Priorities
                .OrderBy(static x => x.Order)
                .Select(static x => new KanjiFormPriorityDto(x.TagName))
                .ToList(),
        };

    private static readonly Expression<Func<Reading, ReadingDto>> ReadingProjection =
        static r => new ReadingDto(r.Text, r.NoKanji)
        {
            Infos = r.Infos
                .OrderBy(static x => x.Order)
                .Select(static x => new ReadingInfoDto(x.TagName))
                .ToList(),
            Priorities = r.Priorities
                .OrderBy(static x => x.Order)
                .Select(static x => new ReadingPriorityDto(x.TagName))
                .ToList(),
            Restrictions = r.Restrictions
                .OrderBy(static x => x.Order)
                .Select(static x => new RestrictionDto(x.KanjiFormText))
                .ToList(),
        };

    private static readonly Expression<Func<Sense, SenseDto>> SenseProjection =
        static s => new SenseDto(s.Note)
        {
            CrossReferences = s.CrossReferences
                .OrderBy(static x => x.Order)
                .Select(static x => new CrossReferenceDto(x.TypeName, x.RefText1, x.RefText2, x.SenseOrder))
                .ToList(),
            Dialects = s.Dialects
                .OrderBy(static x => x.Order)
                .Select(static x => new DialectDto(x.TagName))
                .ToList(),
            Fields = s.Fields
                .OrderBy(static x => x.Order)
                .Select(static x => new FieldDto(x.TagName))
                .ToList(),
            Glosses = s.Glosses
                .OrderBy(static x => x.Order)
                .Select(static x => new GlossDto(x.TypeName, x.Text))
                .ToList(),
            KanjiFormRestrictions = s.KanjiFormRestrictions
                .OrderBy(static x => x.Order)
                .Select(static x => new KanjiFormRestrictionDto(x.KanjiFormText))
                .ToList(),
            LanguageSources = s.LanguageSources
                .OrderBy(static x => x.Order)
                .Select(static x => new LanguageSourceDto(x.Text, x.LanguageCode, x.TypeName, x.IsWasei))
                .ToList(),
            Miscs = s.Miscs
                .OrderBy(static x => x.Order)
                .Select(static x => new MiscDto(x.TagName))
                .ToList(),
            PartsOfSpeech = s.PartsOfSpeech
                .OrderBy(static x => x.Order)
                .Select(static x => new PartOfSpeechDto(x.TagName))
                .ToList(),
            ReadingRestrictions = s.ReadingRestrictions
                .OrderBy(static x => x.Order)
                .Select(static x => new ReadingRestrictionDto(x.ReadingText))
                .ToList(),
        };
}
