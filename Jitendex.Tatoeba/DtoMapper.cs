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
using Jitendex.Dto.Tatoeba;
using Jitendex.Tatoeba.Entities;

namespace Jitendex.Tatoeba;

public static class DtoMapper
{
    public static Dictionary<int, SequenceDto> LoadSequencesWithoutRevisions(Context context, IReadOnlyCollection<int> sequenceIds)
        => context.Sequences
            .AsSplitQuery()
            .Where(seq => sequenceIds.Contains(seq.Id))
            .Select(static seq => new SequenceDto(seq.Id, seq.CreatedDate)
            {
                Entry = seq.Entry == null ? null : new EntryDto
                {
                    EnglishSentenceText = seq.Entry.EnglishSentence == null ? null
                        : seq.Entry.EnglishSentence.Text,
                    JapaneseSentence = seq.Entry.JapaneseSentence == null ? null
                        : new JapaneseSentenceDto
                        {
                            Text = seq.Entry.JapaneseSentence.Text,
                            Segmentations = seq.Entry.JapaneseSentence.Segmentations
                                .AsQueryable()
                                .Select(SegmentationProjection)
                                .ToList(),
                        }
                }
            })
            .ToDictionary(static dto => dto.Id);

    private static Expression<Func<Segmentation, SegmentationDto>> SegmentationProjection =>
        static segmentation => new SegmentationDto
        {
            EnglishSentence = new EnglishSentenceDto(segmentation.EnglishSentence.EntryId)
            {
                Text = segmentation.EnglishSentence.Text
            },
            Tokens = segmentation.Tokens
                .AsQueryable()
                .Select(TokenProjection)
                .ToList()
        };

    private static Expression<Func<Token, TokenDto>> TokenProjection =>
        static token => new TokenDto
        {
            Headword = token.Headword,
            Reading = token.Reading,
            EntryId = token.JmdictEntryId,
            SenseNumber = token.SenseNumber,
            SentenceForm = token.SentenceForm,
            IsPriority = token.IsPriority,
        };
}
