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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Jitendex.SupplementalData;
using Jitendex.SupplementalData.Entities.JMdict;
using Jitendex.JMdict.Entities.EntryItems.SenseItems;

namespace Jitendex.JMdict.Analysis;

internal partial class ReferenceSequencer
{
    private readonly ILogger<ReferenceSequencer> _logger;
    private readonly JmdictContext _jmdictContext;
    private readonly SupplementContext _supplementContext;

    public ReferenceSequencer(ILogger<ReferenceSequencer> logger, JmdictContext jmdictContext, SupplementContext supplementContext) =>
        (_logger, _jmdictContext, _supplementContext) =
        (@logger, @jmdictContext, @supplementContext);

    private readonly record struct ReferenceText(string Text1, string? Text2);
    private readonly record struct EntryInfo(int Id, int SenseCount);

    public void FindCrossReferenceSequenceIds()
    {
        var referenceTextToEntryInfos = GetReferenceTextToEntryInfos();

        var allCrossReferences = _jmdictContext.CrossReferences
            .AsNoTracking()
            .ToList();

        var xrefCache = _supplementContext.CrossReferenceSequences
            .AsNoTracking()
            .ToDictionary(static x => x.ToExportKey());

        var transaction = _supplementContext.Database.BeginTransaction();
        _supplementContext.CrossReferenceSequences.ExecuteDelete();

        foreach (var xref in allCrossReferences)
        {
            var potentialEntryIds = GetPotentialEntryIds(xref, referenceTextToEntryInfos);

            var entryId = potentialEntryIds.Length == 0
                ? null
                : potentialEntryIds.Length == 1
                ? potentialEntryIds[0]
                : CheckCache(xref, potentialEntryIds, xrefCache);

            _supplementContext.CrossReferenceSequences.Add(new()
            {
                SequenceId = xref.EntryId,
                SenseNumber = xref.SenseOrder + 1,
                RefText1 = xref.RefText1,
                RefText2 = xref.RefText2 ?? string.Empty,
                RefSenseNumber = xref.RefSenseNumber,
                RefSequenceId = entryId,
            });
        }

        _supplementContext.SaveChanges();
        transaction.Commit();
    }

    private int? CheckCache(CrossReference xref, int[] potentialIds, Dictionary<string, CrossReferenceSequence> xrefCache)
    {
        int? entryId;
        if (!xrefCache.TryGetValue(xref.ToExportKey(), out var crossRefSeq))
        {
            entryId = null;
        }
        else if (crossRefSeq.RefSequenceId is null)
        {
            entryId = null;
        }
        else if (!potentialIds.Contains((int)crossRefSeq.RefSequenceId))
        {
            entryId = null;
        }
        else
        {
            entryId = crossRefSeq.RefSequenceId;
        }

        if (entryId is null)
        {
            LogAmbiguousReference(xref.ToExportKey(), potentialIds.Length, potentialIds);
        }

        return entryId;
    }

    private int[] GetPotentialEntryIds(CrossReference xref, IReadOnlyDictionary<ReferenceText, List<EntryInfo>> referenceTextToEntries)
    {
        var key = new ReferenceText(xref.RefText1, xref.RefText2);

        if (!referenceTextToEntries.TryGetValue(key, out var entryInfos))
        {
            LogImpossibleReference(xref.ToExportKey());
            return [];
        }

        var possibleTargetEntryIds = entryInfos
            .Where(e => e.Id != xref.EntryId && e.SenseCount >= xref.RefSenseNumber)
            .Select(e => e.Id)
            .ToArray();

        if (possibleTargetEntryIds.Length == 0)
        {
            LogBizarreReference(xref.ToExportKey());
        }

        return possibleTargetEntryIds;
    }

    private IReadOnlyDictionary<ReferenceText, List<EntryInfo>> GetReferenceTextToEntryInfos()
    {
        var entries = _jmdictContext.Entries
            .AsSplitQuery()
            .Select(static e => new
            {
                e.Id,
                Readings = e.Readings.Select(static r => r.Text),
                KanjiForms = e.KanjiForms.Select(static k => k.Text),
                SenseCount = e.Senses.Count(),
            })
            .ToList();

        var dict = new Dictionary<ReferenceText, List<EntryInfo>>(entries.Count * 4);
        foreach (var entry in entries)
        {
            var entryInfo = new EntryInfo(entry.Id, entry.SenseCount);
            foreach (var referenceText in GetReferenceTexts(entry.Readings, entry.KanjiForms))
            {
                if (dict.TryGetValue(referenceText, out var values))
                {
                    values.Add(entryInfo);
                }
                else
                {
                    dict.Add(referenceText, [entryInfo]);
                }
            }
        }
        return dict;
    }

    private static IEnumerable<ReferenceText> GetReferenceTexts(IEnumerable<string> readings, IEnumerable<string> kanjiForms)
    {
        foreach (var kanjiForm in kanjiForms)
        {
            foreach (var reading in readings)
            {
                yield return new ReferenceText(kanjiForm, reading);
            }

            // References in Jmdict sometimes display only the kanji form without a reading.
            yield return new ReferenceText(kanjiForm, null);
        }

        // It is also possible for references to only show the reading,
        // even if valid kanji forms are available.
        foreach (var reading in readings)
        {
            yield return new ReferenceText(reading, null);
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Reference `{CacheKey}` could refer to {Count} possible entries: {EntryIds}")]
    partial void LogAmbiguousReference(string cacheKey, int count, int[] entryIds);

    [LoggerMessage(LogLevel.Warning,
    "Reference `{CacheKey}` refers to an entry that does not exist.")]
    partial void LogImpossibleReference(string cacheKey);

    [LoggerMessage(LogLevel.Warning,
    "Reference `{CacheKey}` is invalid either because it points to itself or to an invalid sense number")]
    partial void LogBizarreReference(string cacheKey);
}
