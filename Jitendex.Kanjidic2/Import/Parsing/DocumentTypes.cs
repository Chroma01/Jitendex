/*
Copyright (c) 2025 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

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

using Jitendex.Kanjidic2.Entities;
using Microsoft.Extensions.Logging;

namespace Jitendex.Kanjidic2.Import.Parsing;

internal partial class DocumentTypes
{
    private readonly ILogger<DocumentTypes> _logger;
    private readonly Dictionary<(Type, string), IKeyword> _cache = [];
    private readonly Dictionary<(Type, string), string> _nameToDescription = [];

    public DocumentTypes(ILogger<DocumentTypes> logger)
    {
        _logger = logger;
        RegisterKeywords();
    }

    public T GetByName<T>(string? name) where T : IKeyword, new()
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = Guid.NewGuid().ToString();
            Register<T>(name, string.Empty);
        }

        var cacheKey = (typeof(T), name);
        if (_cache.TryGetValue(cacheKey, out IKeyword? keyword))
        {
            return (T)keyword;
        }

        T newKeyword;

        if (_nameToDescription.TryGetValue(cacheKey, out string? value))
        {
            newKeyword = new T { Name = name, Description = value, IsCorrupt = value == string.Empty };
        }
        else
        {
            LogUnregisteredKeywordName(name, typeof(T).Name);
            newKeyword = new T { Name = name, Description = string.Empty, IsCorrupt = true };
        }

        _cache.Add(cacheKey, newKeyword);
        return newKeyword;
    }

    [LoggerMessage(LogLevel.Warning,
    "Keyword name `{Name}` for type `{TypeName}` was not registered with a description before use.")]
    partial void LogUnregisteredKeywordName(string name, string typeName);


    public Dictionary<string, CodepointType> CodepointTypes() => GetKeywords<CodepointType>();
    public Dictionary<string, DictionaryType> DictionaryTypes() => GetKeywords<DictionaryType>();
    public Dictionary<string, QueryCodeType> QueryCodeTypes() => GetKeywords<QueryCodeType>();
    public Dictionary<string, MisclassificationType> MisclassificationTypes() => GetKeywords<MisclassificationType>();
    public Dictionary<string, RadicalType> RadicalTypes() => GetKeywords<RadicalType>();
    public Dictionary<string, ReadingType> ReadingTypes() => GetKeywords<ReadingType>();
    public Dictionary<string, VariantType> VariantTypes() => GetKeywords<VariantType>();

    private Dictionary<string, T> GetKeywords<T>() where T : IKeyword => _cache.Values
        .Where(static keyword => keyword is T)
        .Select(static keyword => new KeyValuePair<string, T>(keyword.Name, (T)keyword))
        .ToDictionary();

    private void Register<T>(string name, string description) where T : IKeyword
    {
        _nameToDescription.Add((typeof(T), name), description);
    }

    private void RegisterKeywords()
    {
        foreach (var (name, description) in CodepointKeywords)
            Register<CodepointType>(name, description);

        foreach (var (name, description) in DictionaryKeywords)
            Register<DictionaryType>(name, description);

        foreach (var (name, description) in QueryCodeKeywords)
            Register<QueryCodeType>(name, description);

        foreach (var (name, description) in QueryCodeMisclassifications)
            Register<MisclassificationType>(name, description);

        foreach (var (name, description) in RadicalKeywords)
            Register<RadicalType>(name, description);

        foreach (var (name, description) in ReadingKeywords)
            Register<ReadingType>(name, description);

        foreach (var (name, description) in VariantKeywords)
            Register<VariantType>(name, description);
    }

    private static readonly Dictionary<string, string> CodepointKeywords = new()
    {
        ["jis208"] = "JIS X 0208-1997 - kuten coding (p-nn-nn)",
        ["jis212"] = "JIS X 0212-1990 - kuten coding (p-nn-nn)",
        ["jis213"] = "JIS X 0213-2000 - kuten coding (p-nn-nn)",
        ["ucs"] = "Unicode 4.0 - hex coding (4 or 5 hexadecimal digits)",
    };

    private static readonly Dictionary<string, string> DictionaryKeywords = new()
    {
        ["busy_people"] = "\"Japanese For Busy People\" vols I-III, published by the AJLT. The codes are the volume.chapter.",
        ["crowley"] = "\"The Kanji Way to Japanese Language Power\" by Dale Crowley.",
        ["gakken"] = "\"A New Dictionary of Kanji Usage\" (Gakken)",
        ["halpern_kkd"] = "\"Kodansha Kanji Dictionary\", (2nd Ed. of the NJECD) edited by Jack Halpern.",
        ["halpern_kkld"] = "\"Kanji Learners Dictionary\" (Kodansha) edited by Jack Halpern.",
        ["halpern_kkld_2ed"] = "\"Kanji Learners Dictionary\" (Kodansha), 2nd edition (2013) edited by Jack Halpern.",
        ["halpern_njecd"] = "\"New Japanese-English Character Dictionary\", edited by Jack Halpern.",
        ["heisig"] = "\"Remembering The Kanji\" by James Heisig.",
        ["heisig6"] = "\"Remembering The Kanji, Sixth Ed.\" by James Heisig.",
        ["henshall"] = "\"A Guide To Remembering Japanese Characters\" by Kenneth G. Henshall.",
        ["henshall3"] = "\"A Guide To Reading and Writing Japanese\" 3rd edition, edited by Henshall, Seeley and De Groot.",
        ["jf_cards"] = "Japanese Kanji Flashcards, by Max Hodges and Tomoko Okazaki. (Series 1)",
        ["kanji_in_context"] = "\"Kanji in Context\" by Nishiguchi and Kono.",
        ["kodansha_compact"] = "the \"Kodansha Compact Kanji Guide\".",
        ["maniette"] = "codes from Yves Maniette's \"Les Kanjis dans la tete\" French adaptation of Heisig.",
        ["moro"] = "\"Daikanwajiten\" compiled by Morohashi.",
        ["nelson_c"] = "\"Modern Reader's Japanese-English Character Dictionary\", edited by Andrew Nelson (now published as the \"Classic\" Nelson).",
        ["nelson_n"] = "\"The New Nelson Japanese-English Character Dictionary\", edited by John Haig.",
        ["oneill_kk"] = "\"Essential Kanji\" by P.G. O'Neill.",
        ["oneill_names"] = "\"Japanese Names\", by P.G. O'Neill.",
        ["sakade"] = "\"A Guide To Reading and Writing Japanese\" edited by Florence Sakade.",
        ["sh_kk"] = "\"Kanji and Kana\" by Spahn and Hadamitzky.",
        ["sh_kk2"] = "\"Kanji and Kana\" by Spahn and Hadamitzky (2011 edition).",
        ["tutt_cards"] = "Tuttle Kanji Cards, compiled by Alexander Kask.",
    };

    private static readonly Dictionary<string, string> QueryCodeKeywords = new()
    {
        ["deroo"] = "the codes developed by the late Father Joseph De Roo, and published in  his book \"2001 Kanji\" (Bonjinsha). Fr De Roo gave his permission for these codes to be included.",
        ["four_corner"] = "the \"Four Corner\" code for the kanji. This is a code invented by Wang Chen in 1928. See the KANJIDIC documentation for  an overview of  the Four Corner System.",
        ["sh_desc"] = "the descriptor codes for The Kanji Dictionary (Tuttle 1996) by Spahn and Hadamitzky. They are in the form nxnn.n, e.g. 3k11.2, where the  kanji has 3 strokes in the identifying radical, it is radical \"k\" in the SH classification system, there are 11 other strokes, and it is the 2nd kanji in the 3k11 sequence.",
        ["skip"] = "Halpern's SKIP (System of Kanji Indexing by Patterns) code. The format is n-nn-nn.",
    };

    private static readonly Dictionary<string, string> QueryCodeMisclassifications = new()
    {
        ["posn"] = "a mistake in the division of the kanji",
        ["stroke_and_posn"] = "mistakes in both division and strokes",
        ["stroke_count"] = "a mistake in the number of strokes",
        ["stroke_diff"] = "ambiguous stroke counts depending on glyph",
    };

    private static readonly Dictionary<string, string> RadicalKeywords = new()
    {
        ["classical"] = "based on the system first used in the KangXi Zidian. The Shibano \"JIS Kanwa Jiten\" is used as the reference source.",
        ["nelson_c"] = "as used in the Nelson \"Modern Japanese-English Character Dictionary\" (i.e. the Classic, not the New Nelson). This will only be used where Nelson reclassified the kanji.",
    };

    private static readonly Dictionary<string, string> ReadingKeywords = new()
    {
        ["ja_kun"] = "the \"kun\" Japanese reading of the kanji, usually in hiragana.",
        ["ja_on"] = "the \"on\" Japanese reading of the kanji, in katakana.",
        ["korean_h"] = "the Korean reading(s) of the kanji in hangul.",
        ["korean_r"] = "the romanized form of the Korean reading(s) of the kanji. The readings are in the (Republic of Korea) Ministry of Education style of romanization.",
        ["pinyin"] = "the modern PinYin romanization of the Chinese reading of the kanji. The tones are represented by a concluding digit.",
        ["vietnam"] = "the Vietnamese readings supplied by Minh Chau Pham.",
    };

    private static readonly Dictionary<string, string> VariantKeywords = new()
    {
        ["deroo"] = "De Roo number - numeric",
        ["jis208"] = "in JIS X 0208 - kuten coding",
        ["jis212"] = "in JIS X 0212 - kuten coding",
        ["jis213"] = "in JIS X 0213 - kuten coding",
        ["nelson_c"] = "\"Classic\" Nelson - numeric",
        ["njecd"] = "Halpern NJECD index number - numeric",
        ["oneill"] = "Japanese Names (O'Neill) - numeric",
        ["s_h"] = "The Kanji Dictionary (Spahn & Hadamitzky) - descriptor",
        ["ucs"] = "Unicode codepoint- hex",
    };
}
