/*
Copyright (c) 2025 Doublevil
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

namespace Jitendex.Furigana.Models;

/// <summary>
/// Represents an individual part of the reading of a word or expression.
/// </summary>
public class ReadingPart
{
    /// <summary>
    /// Gets or sets the text of the part (with kanji when applicable).
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the furigana of the part, if necessary (will be empty when the text is plain kana).
    /// </summary>
    public string? Furigana { get; set; }
}
