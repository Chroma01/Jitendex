/*
Copyright (c) 2026 Stephen Kraus
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

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.SupplementalData.Entities.JMdict;

[Table(nameof(CrossReferenceSequence))]
[PrimaryKey(nameof(SequenceId), nameof(SenseNumber), nameof(RefText1), nameof(RefText2), nameof(RefSenseNumber))]
public sealed class CrossReferenceSequence
{
    public required int SequenceId { get; init; }
    public required int SenseNumber { get; init; }
    public required string RefText1 { get; init; }
    public required string RefText2 { get; init; }
    public required int RefSenseNumber { get; init; }
    public required int? RefSequenceId { get; init; }

    public string ToExportKey()
        => RefText2 == string.Empty
            ? $"{SequenceId}・{SenseNumber}・{RefText1}・{RefSenseNumber}"
            : $"{SequenceId}・{SenseNumber}・{RefText1}【{RefText2}】・{RefSenseNumber}";
}
