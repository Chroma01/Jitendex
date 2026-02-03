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

using Microsoft.Extensions.Logging;

namespace Jitendex.JMdict.Analysis;

internal partial class Analyzer
{
    private readonly ILogger<Analyzer> _logger;
    private readonly ReadingBridger _readingBridger;
    private readonly ReferenceSequencer _referenceSequencer;

    public Analyzer(ILogger<Analyzer> logger, ReadingBridger readingBridger, ReferenceSequencer referenceSequencer) =>
        (_logger, _readingBridger, _referenceSequencer) =
        (@logger, @readingBridger, @referenceSequencer);

    public void Analyze()
    {
        _readingBridger.BridgeReadingsToKanjiForms();
        _referenceSequencer.FindCrossReferenceSequenceIds();
    }
}
