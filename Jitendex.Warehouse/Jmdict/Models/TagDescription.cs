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

namespace Jitendex.Warehouse.Jmdict.Models;

public interface ITagDescription
{
    string Id { get; set; }
    string Text { get; set; }
}

/*
Cannot use the `required` modifier on the class properties below because we
want to use these classes as type parameters with the `new()` constraint.
https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required
*/

public class ReadingInfoTagDescription : ITagDescription
{
    public string Id { get; set; } = null!;
    public string Text { get; set; } = null!;
}

public class KanjiFormInfoTagDescription : ITagDescription
{
    public string Id { get; set; } = null!;
    public string Text { get; set; } = null!;
}

public class PartOfSpeechTagDescription : ITagDescription
{
    public string Id { get; set; } = null!;
    public string Text { get; set; } = null!;
}

public class FieldTagDescription : ITagDescription
{
    public string Id { get; set; } = null!;
    public string Text { get; set; } = null!;
}

public class MiscTagDescription : ITagDescription
{
    public string Id { get; set; } = null!;
    public string Text { get; set; } = null!;
}

public class DialectTagDescription : ITagDescription
{
    public string Id { get; set; } = null!;
    public string Text { get; set; } = null!;
}
