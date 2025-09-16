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

using Jitendex.Furigana.Solver;

namespace Jitendex.Furigana.Test.SolverTests;

[TestClass]
public class SingleNonKana
{
    private static readonly IterationSolver _solver = new([], []);

    private static readonly (string, string, string)[] _data = new[]
    {
        // Single kanji
        ("腹", "はら", "[腹|はら]"),
        ("嗹", "れん", "[嗹|れん]"),

        // Single non-kanji
        ("◯", "おおきなまる", "[◯|おおきなまる]"),
        ("々", "とき", "[々|とき]"),

        // Suffixed kanji
        ("難しい", "むずかしい", "[難|むずか]しい"),

        // Prefixed kanji
        ("ばね秤", "ばねばかり", "ばね[秤|ばかり]"),

        // Prefixed and suffixed kanji with one furigana character
        ("ぜんまい仕かけ", "ぜんまいじかけ", "ぜんまい[仕|じ]かけ"),

        // Prefixed and suffixed kanji with two furigana characters
        ("ありがたい事に", "ありがたいことに", "ありがたい[事|こと]に"),

        // Non-normalized text (all have the same reading)
        ("アリガタイ事ニ", "ありがたいことに", "アリガタイ[事|こと]ニ"),
        ("ありがたい事ニ", "ありがたいことに", "ありがたい[事|こと]ニ"),
        ("アリガタイ事に", "ありがたいことに", "アリガタイ[事|こと]に"),
        ("アリガタイ事ニ", "ありがたいことに", "アリガタイ[事|こと]ニ"),
        ("アりガたイ事ニ", "ありがたいことに", "アりガたイ[事|こと]ニ"),

        // Furigana written in katakana
        ("嗹", "レン", "[嗹|レン]"),
        ("ありがたい事に", "ありがたいコトに", "ありがたい[事|コト]に"),
    };

    [TestMethod]
    public void Test()
    {
        SolverTestMethods.TestSolvable(_solver, _data);
    }
}
