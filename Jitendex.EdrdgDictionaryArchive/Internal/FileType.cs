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

using static Jitendex.EdrdgDictionaryArchive.DictionaryFile;

namespace Jitendex.EdrdgDictionaryArchive.Internal;

internal readonly ref struct FileType
{
    public readonly ReadOnlySpan<char> Name { get; }
    public readonly ReadOnlySpan<char> DirectoryName { get; }
    public readonly ReadOnlySpan<char> CompressedName { get; }

    public FileType(DictionaryFile file)
    {
        Name = GetFileName(file);
        DirectoryName = ToDirectoryName(Name);
        CompressedName = $"{Name}.br";
    }

    private static ReadOnlySpan<char> GetFileName(DictionaryFile file) => file switch
    {
        JMdict => "JMdict",
        JMdict_e => "JMdict_e",
        JMdict_e_examp => "JMdict_e_examp",
        JMnedict => "JMnedict.xml",
        kanjidic2 => "kanjidic2.xml",
        examples => "examples.utf",
        _ => throw new ArgumentOutOfRangeException(nameof(file))
    };

    /// <summary>
    /// Replace the '.' characters in a filename with underscores to get a folder name.
    /// </summary>
    private static ReadOnlySpan<char> ToDirectoryName(ReadOnlySpan<char> name) => string.Create
    (
        length: name.Length,
        state: name,
        action: static (destination, filename)
            => filename.Replace(destination, oldValue: '.', newValue: '_')
    );
}
