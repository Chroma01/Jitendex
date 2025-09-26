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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jitendex.Import.Kanjidic2.Models;

[Table(nameof(Header))]
public class Header
{
    [Key]
    public required string DatabaseVersion { get; set; }
    public required string FileVersion { get; set; }
    public required string DateOfCreation { get; set; }

    internal const string XmlTagName = "header";
    internal const string file_XmlTagName = "file_version";
    internal const string database_XmlTagName = "database_version";
    internal const string date_XmlTagName = "date_of_creation";
}
