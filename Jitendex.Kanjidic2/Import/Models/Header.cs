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

namespace Jitendex.Kanjidic2.Import.Models;

internal sealed record Header
{
    public required string DatabaseVersion { get; set; }
    public required string FileVersion { get; set; }
    public required DateOnly DateOfCreation { get; set; }

    public const string XmlTagName = "header";
    public const string file_XmlTagName = "file_version";
    public const string database_XmlTagName = "database_version";
    public const string date_XmlTagName = "date_of_creation";
}
