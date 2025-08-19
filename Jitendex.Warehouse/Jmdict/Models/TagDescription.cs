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

    static abstract ITagDescription Factory(string id, string text);
}

public class ReadingInfoTagDescription : ITagDescription
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    public static ITagDescription Factory(string id, string text)
        => new ReadingInfoTagDescription { Id = id, Text = text };
}

public class KanjiFormInfoTagDescription : ITagDescription
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    public static ITagDescription Factory(string id, string text)
        => new KanjiFormInfoTagDescription { Id = id, Text = text };
}

public class PartOfSpeechTagDescription : ITagDescription
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    public static ITagDescription Factory(string id, string text)
        => new PartOfSpeechTagDescription { Id = id, Text = text };
}

public class FieldTagDescription : ITagDescription
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    public static ITagDescription Factory(string id, string text)
        => new FieldTagDescription { Id = id, Text = text };
}

public class MiscTagDescription : ITagDescription
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    public static ITagDescription Factory(string id, string text)
        => new MiscTagDescription { Id = id, Text = text };
}

public class DialectTagDescription : ITagDescription
{
    public required string Id { get; set; }
    public required string Text { get; set; }

    public static ITagDescription Factory(string id, string text)
        => new DialectTagDescription { Id = id, Text = text };
}
