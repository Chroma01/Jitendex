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

namespace Jitendex.EdrdgDictionaryArchive;

internal readonly ref struct FileType
{
    public readonly ReadOnlySpan<char> Name { get; }
    public readonly ReadOnlySpan<char> DirectoryName { get; }
    public readonly ReadOnlySpan<char> CompressedName { get; }

    public FileType(ReadOnlySpan<char> name)
    {
        if (!IsValid(name))
        {
            throw new ArgumentException($"Invalid file type '{name}'", nameof(name));
        }
        Name = name;
        DirectoryName = ToDirectoryName(name);
        CompressedName = $"{name}.br";
    }

    private static bool IsValid(ReadOnlySpan<char> name) => name switch
    {
        "JMdict" or
        "JMdict_e" or
        "JMdict_e_examp" or
        "JMnedict.xml" or
        "kanjidic2.xml" or
        "examples.utf" => true,
        _ => false
    };

    private static ReadOnlySpan<char> ToDirectoryName(ReadOnlySpan<char> name) => string.Create
    (
        length: name.Length,
        state: name,
        action: static (buffer, filename) => filename.Replace(buffer, '.', '_')
    );
}
