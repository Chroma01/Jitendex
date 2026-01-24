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
    public static Dictionary<int, SequenceDto> LoadSequencesWithoutRevisions(Context context, IReadOnlyCollection<int> sequenceIds)
        => context.Sequences
            .AsNoTracking()
            .AsSplitQuery()
            .Where(seq => sequenceIds.Contains(seq.Id))
            .Select(static seq => new SequenceDto(seq.Id, seq.CreatedDate)
            {
                Entry = seq.Entry == null ? null : new EntryDto
                {
                    KanjiForms = seq.Entry.KanjiForms
                        .AsQueryable()
                        .OrderBy(static kanjiForm => kanjiForm.Order)
                        .Select(KanjiFormProjection)
                        .ToList(),
                    Readings = seq.Entry.Readings
                        .AsQueryable()
                        .OrderBy(static reading => reading.Order)
                        .Select(ReadingProjection)
                        .ToList(),
                    Senses = seq.Entry.Senses
                        .AsQueryable()
                        .OrderBy(static sense => sense.Order)
                        .Select(SenseProjection)
                        .ToList()
                }
            })
            .ToDictionary(static dto => dto.Id);

    private static readonly Expression<Func<KanjiForm, KanjiFormDto>> KanjiFormProjection =
        static kanjiForm => new KanjiFormDto(kanjiForm.Text)
        {
            Infos = kanjiForm.Infos
                .OrderBy(static info => info.Order)
                .Select(static info => new KanjiFormInfoDto(info.TagName))
                .ToList(),
            Priorities = kanjiForm.Priorities
                .OrderBy(static prio => prio.Order)
                .Select(static prio => new KanjiFormPriorityDto(prio.TagName))
                .ToList(),
        };

    private static readonly Expression<Func<Reading, ReadingDto>> ReadingProjection =
        static reading => new ReadingDto(reading.Text, reading.NoKanji)
        {
            Infos = reading.Infos
                .OrderBy(static info => info.Order)
                .Select(static info => new ReadingInfoDto(info.TagName))
                .ToList(),
            Priorities = reading.Priorities
                .OrderBy(static prio => prio.Order)
                .Select(static prio => new ReadingPriorityDto(prio.TagName))
                .ToList(),
            Restrictions = reading.Restrictions
                .OrderBy(static rstr => rstr.Order)
                .Select(static rstr => new RestrictionDto(rstr.KanjiFormText))
                .ToList(),
        };

    private static readonly Expression<Func<Sense, SenseDto>> SenseProjection =
        static sense => new SenseDto(sense.Note)
        {
            CrossReferences = sense.CrossReferences
                .OrderBy(static x => x.Order)
                .Select(static x => new CrossReferenceDto(x.TypeName, x.RefText1, x.RefText2, x.SenseOrder))
                .ToList(),
            Dialects = sense.Dialects
                .OrderBy(static dia => dia.Order)
                .Select(static dia => new DialectDto(dia.TagName))
                .ToList(),
            Fields = sense.Fields
                .OrderBy(static fld => fld.Order)
                .Select(static fld => new FieldDto(fld.TagName))
                .ToList(),
            Glosses = sense.Glosses
                .OrderBy(static gloss => gloss.Order)
                .Select(static gloss => new GlossDto(gloss.TypeName, gloss.Text))
                .ToList(),
            KanjiFormRestrictions = sense.KanjiFormRestrictions
                .OrderBy(static rstr => rstr.Order)
                .Select(static rstr => new KanjiFormRestrictionDto(rstr.KanjiFormText))
                .ToList(),
            LanguageSources = sense.LanguageSources
                .OrderBy(static l => l.Order)
                .Select(static l => new LanguageSourceDto(l.Text, l.LanguageCode, l.TypeName, l.IsWasei))
                .ToList(),
            Miscs = sense.Miscs
                .OrderBy(static m => m.Order)
                .Select(static m => new MiscDto(m.TagName))
                .ToList(),
            PartsOfSpeech = sense.PartsOfSpeech
                .OrderBy(static pos => pos.Order)
                .Select(static pos => new PartOfSpeechDto(pos.TagName))
                .ToList(),
            ReadingRestrictions = sense.ReadingRestrictions
                .OrderBy(static rstr => rstr.Order)
                .Select(static restr => new ReadingRestrictionDto(restr.ReadingText))
                .ToList(),
        };
}
