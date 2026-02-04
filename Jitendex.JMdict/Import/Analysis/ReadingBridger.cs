/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Jitendex.SupplementalData;
using Jitendex.SupplementalData.Entities.JMdict;

namespace Jitendex.JMdict.Import.Analysis;

internal partial class ReadingBridger
{
    private readonly ILogger<ReadingBridger> _logger;
    private readonly JmdictContext _jmdictContext;
    private readonly SupplementContext _supplementContext;

    public ReadingBridger(ILogger<ReadingBridger> logger, JmdictContext jmdictContext, SupplementContext supplementContext) =>
        (_logger, _jmdictContext, _supplementContext) =
        (@logger, @jmdictContext, @supplementContext);

    private readonly record struct ReadingData(int Order, string Text, bool NoKanji, bool IsHidden, ImmutableArray<string> Restrictions);
    private readonly record struct KanjiFormData(int Order, string Text);
    private readonly record struct Bridge(int EntryId, int ReadingOrder, int KanjiFormOrder);

    public void BridgeReadingsToKanjiForms()
    {
        var bridges = GetBridges();
        WriteBridgesToDatabase(bridges);
    }

    private List<Bridge> GetBridges()
    {
        var entries = _jmdictContext.Entries
            .AsSplitQuery()
            .Select(static e => new
            {
                e.Id,
                Readings = e.Readings
                    .Select(static r => new ReadingData
                    (
                        r.Order,
                        r.Text,
                        r.NoKanji,
                        IsHidden: r.Infos
                            .Select(static i => i.TagName)
                            .Any(static t => t == "sk"),
                        Restrictions: r.Restrictions
                            .Select(static x => x.KanjiFormText)
                            .ToImmutableArray()
                    )),
                KanjiFormInfos = e.KanjiForms
                    .Where(static k => k.Infos.All(static i => i.TagName != "sK"))
                    .Select(static k => new KanjiFormData(k.Order, k.Text))
                    .ToImmutableArray(),
            });

        var bridges = new List<Bridge>(250_000);

        foreach (var entry in entries)
        {
            var usedKanjiFormOrders = new HashSet<int>(entry.KanjiFormInfos.Length);
            foreach (var reading in entry.Readings)
            {
                CheckForRedundancies(entry.Id, entry.KanjiFormInfos.Length, reading);
                if (entry.KanjiFormInfos.Length == 0 || reading.NoKanji || reading.IsHidden)
                {
                    continue;
                }
                var kanjiFormOrders = reading.Restrictions.Length > 0
                    ? GetRestrictionOrders(entry.Id, reading, entry.KanjiFormInfos)
                    : entry.KanjiFormInfos.Select(static k => k.Order).ToArray();
                foreach (var order in kanjiFormOrders)
                {
                    usedKanjiFormOrders.Add(order);
                    bridges.Add(new(entry.Id, reading.Order, order));
                }
            }
            if (usedKanjiFormOrders.Count != entry.KanjiFormInfos.Length)
            {
                LogOrphanKanjiForms(entry.Id);
            }
        }

        return bridges;
    }

    private int[] GetRestrictionOrders(int entryId, in ReadingData reading, in ImmutableArray<KanjiFormData> kanjiForms)
    {
        var restrictions = reading.Restrictions;
        var orders = kanjiForms
            .Where(k => restrictions.Contains(k.Text))
            .Select(static k => k.Order)
            .ToArray();

        if (orders.Length != reading.Restrictions.Length)
        {
            LogInvalidRestriction(entryId, reading.Text);
        }

        return orders;
    }

    private void CheckForRedundancies(int entryId, int visibleKanjiFormCount, in ReadingData reading)
    {
        // A reading shouldn't have both [NoKanji] and restrictions.
        int count0 = (reading.NoKanji ? 1 : 0) + (reading.Restrictions.Length > 0 ? 1 : 0);

        // If the reading is hidden, it shouldn't have [NoKanji] or restrictions.
        int count1 = (reading.IsHidden ? 1 : 0) + count0;

        // If there are no visible kanji forms, it shouldn't have [NoKanji] or restrictions
        int count2 = (visibleKanjiFormCount == 0 ? 1 : 0) + count0;

        if (count0 > 1 || count1 > 1 || count2 > 1)
        {
            LogRedundantRestrictions(entryId, reading.Text);
        }
    }

    private void WriteBridgesToDatabase(List<Bridge> bridges)
    {
        using var transaction = _supplementContext.Database.BeginTransaction();
        _supplementContext.ReadingKanjiFormBridges.ExecuteDelete();

        using var command = _supplementContext.Database.GetDbConnection().CreateCommand();
        command.CommandText =
            $"""
            INSERT INTO "{nameof(ReadingKanjiFormBridge)}"
            ( "{nameof(ReadingKanjiFormBridge.SequenceId)}"
            , "{nameof(ReadingKanjiFormBridge.ReadingOrder)}"
            , "{nameof(ReadingKanjiFormBridge.KanjiFormOrder)}"
            ) VALUES (@0, @1, @2);
            """;

        foreach (var bridge in bridges)
        {
            command.Parameters.AddRange(new SqliteParameter[]
            {
                new("@0", bridge.EntryId),
                new("@1", bridge.ReadingOrder),
                new("@2", bridge.KanjiFormOrder),
            });
            command.ExecuteNonQuery();
            command.Parameters.Clear();
        }

        _supplementContext.SaveChanges();
        transaction.Commit();
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry ID {EntryId} reading `{Reading}` contains redundant restrictions")]
    protected partial void LogRedundantRestrictions(int entryId, string reading);

    [LoggerMessage(LogLevel.Warning,
    "Entry ID {EntryId} reading `{Reading}` contains a restriction to an invalid kanji form")]
    protected partial void LogInvalidRestriction(int entryId, string reading);

    [LoggerMessage(LogLevel.Warning,
    "Entry ID {EntryId} contains a visible kanji form without a corresponding reading")]
    protected partial void LogOrphanKanjiForms(int entryId);
}
