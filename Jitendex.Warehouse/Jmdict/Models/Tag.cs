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

public interface ITag
{
    string Id { get; set; }
    string Text { get; set; }

    static abstract internal ITag New(string id, string text);
}

public class ReadingInfoTag : ITag
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    static ITag ITag.New(string id, string text)
        => new ReadingInfoTag { Id = id, Text = text };
}

public class KanjiFormInfoTag : ITag
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    static ITag ITag.New(string id, string text)
        => new KanjiFormInfoTag { Id = id, Text = text };
}

public class PartOfSpeechTag : ITag
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    static ITag ITag.New(string id, string text)
        => new PartOfSpeechTag { Id = id, Text = text };
}

public class FieldTag : ITag
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    static ITag ITag.New(string id, string text)
        => new FieldTag { Id = id, Text = text };
}

public class MiscTag : ITag
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    static ITag ITag.New(string id, string text)
        => new MiscTag { Id = id, Text = text };
}

public class DialectTag : ITag
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    static ITag ITag.New(string id, string text)
        => new DialectTag { Id = id, Text = text };
}
