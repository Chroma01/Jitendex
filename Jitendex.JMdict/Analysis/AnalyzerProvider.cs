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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Jitendex.SupplementalData;

namespace Jitendex.JMdict.Analysis;

internal static class AnalyzerProvider
{
    public static Analyzer GetAnalyzer() => new ServiceCollection()
        .AddLogging(static builder =>
            builder.AddSimpleConsole(static options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }))

        // Databases
        .AddDbContext<JmdictContext>()
        .AddDbContext<SupplementContext>()

        // Analysis units
        .AddTransient<CrossReferenceTextParser>()
        .AddTransient<ReferenceSequencer>()
        .AddTransient<ReadingBridger>()

        // Build and return the service
        .AddTransient<Analyzer>()
        .BuildServiceProvider()
        .GetRequiredService<Analyzer>();
}
