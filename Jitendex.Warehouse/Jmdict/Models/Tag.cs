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

namespace Jitendex.Warehouse.Jmdict.Models;

public interface ITag
{
    string Code { get; set; }
    string Description { get; set; }
}

/*
Cannot use the `required` modifier on the class properties below because we
want to use these classes as type parameters with the `new()` constraint.
https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required
*/

[Table("Jmdict.ReadingInfoTags")]
public class ReadingInfoTag : ITag
{
    [Key]
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
}

[Table("Jmdict.KanjiInfoTags")]
public class KanjiInfoTag : ITag
{
    [Key]
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
}
