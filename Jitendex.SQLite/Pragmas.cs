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

namespace Jitendex.SQLite;

/// <remarks>
/// See: https://www.sqlite.org/pragma.html
/// </remarks>
public static class Pragmas
{
    public static readonly string FastNewDatabase =
        """
        PRAGMA synchronous = OFF;
        PRAGMA journal_mode = OFF;
        PRAGMA temp_store = MEMORY;
        PRAGMA cache_size = -200000;
        PRAGMA locking_mode = EXCLUSIVE;
        """;

    public static readonly string DeferForeignKeys =
        "PRAGMA defer_foreign_keys = ON;";
}
