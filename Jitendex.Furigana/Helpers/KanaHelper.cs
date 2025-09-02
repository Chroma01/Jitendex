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

using System.Text.RegularExpressions;

namespace Jitendex.Furigana.Helpers;

public static class KanaHelper
{
    #region Fields

    private static readonly char[] KatakanaDictionary =
    [
        'ッ','チ','シ','ツ','ヅ','ヂ','ヮ','ャ','ィ','ュ','ェ','ョ','カ',
        'キ','ク','ケ','コ','サ','ス','セ','ソ','タ','テ','ト','ナ','ニ',
        'ヌ','ネ','ノ','ハ','ヒ','フ','ヘ','ホ','マ','ミ','ム','メ','モ',
        'ヤ','ユ','ヨ','ラ','リ','ル','レ','ロ','ワ','ヲ','ガ','ギ','グ',
        'ゲ','ゴ','ジ','ダ','デ','ド','バ','ビ','ブ','ベ','ボ','パ','ピ',
        'プ','ペ','ポ','ザ','ズ','ゼ','ゾ','ァ','ィ','ゥ','ェ','ォ','ン',
        'ア','イ','ウ','エ','オ','ー','ヴ','・','、','。','ヱ','ヰ'
    ];

    private static readonly char[] HiraganaDictionary =
    [
        'っ','ち','し','つ','づ','ぢ','ゎ','ゃ','ぃ','ゅ','ぇ','ょ','か',
        'き','く','け','こ','さ','す','せ','そ','た','て','と','な','に',
        'ぬ','ね','の','は','ひ','ふ','へ','ほ','ま','み','む','め','も',
        'や','ゆ','よ','ら','り','る','れ','ろ','わ','を','が','ぎ','ぐ',
        'げ','ご','じ','だ','で','ど','ば','び','ぶ','べ','ぼ','ぱ','ぴ',
        'ぷ','ぺ','ぽ','ざ','ず','ぜ','ぞ','ぁ','ぃ','ぅ','ぇ','ぉ','ん',
        'あ','い','う','え','お','ー','・','、','。','ゑ','ゐ'
    ];

    #endregion

    #region Conversion

    #region Public

    /// <summary>
    /// Converts a kana string to hiragana.
    /// </summary>
    /// <param name="kana">
    /// Input kana string.
    /// May be katakana, hiragana, romaji or mixed.
    /// </param>
    /// <returns>Output hiragana string.</returns>
    public static string ToHiragana(string kana)
    {
        kana = RomajiToKana(kana);

        kana = kana.Replace("ッ", "っ");
        kana = kana.Replace("チ", "ち");
        kana = kana.Replace("シ", "し");
        kana = kana.Replace("ツ", "つ");
        kana = kana.Replace("ヅ", "づ");
        kana = kana.Replace("ヂ", "ぢ");
        kana = kana.Replace("ヮ", "ゎ");
        kana = kana.Replace("ャ", "ゃ");
        kana = kana.Replace("ィ", "ぃ");
        kana = kana.Replace("ュ", "ゅ");
        kana = kana.Replace("ェ", "ぇ");
        kana = kana.Replace("ョ", "ょ");
        kana = kana.Replace("カ", "か");
        kana = kana.Replace("キ", "き");
        kana = kana.Replace("ク", "く");
        kana = kana.Replace("ケ", "け");
        kana = kana.Replace("コ", "こ");
        kana = kana.Replace("サ", "さ");
        kana = kana.Replace("ス", "す");
        kana = kana.Replace("セ", "せ");
        kana = kana.Replace("ソ", "そ");
        kana = kana.Replace("タ", "た");
        kana = kana.Replace("テ", "て");
        kana = kana.Replace("ト", "と");
        kana = kana.Replace("ナ", "な");
        kana = kana.Replace("ニ", "に");
        kana = kana.Replace("ヌ", "ぬ");
        kana = kana.Replace("ネ", "ね");
        kana = kana.Replace("ノ", "の");
        kana = kana.Replace("ハ", "は");
        kana = kana.Replace("ヒ", "ひ");
        kana = kana.Replace("フ", "ふ");
        kana = kana.Replace("ヘ", "へ");
        kana = kana.Replace("ホ", "ほ");
        kana = kana.Replace("マ", "ま");
        kana = kana.Replace("ミ", "み");
        kana = kana.Replace("ム", "む");
        kana = kana.Replace("メ", "め");
        kana = kana.Replace("モ", "も");
        kana = kana.Replace("ヤ", "や");
        kana = kana.Replace("ユ", "ゆ");
        kana = kana.Replace("ヨ", "よ");
        kana = kana.Replace("ラ", "ら");
        kana = kana.Replace("リ", "り");
        kana = kana.Replace("ル", "る");
        kana = kana.Replace("レ", "れ");
        kana = kana.Replace("ロ", "ろ");
        kana = kana.Replace("ワ", "わ");
        kana = kana.Replace("ヲ", "を");
        kana = kana.Replace("ガ", "が");
        kana = kana.Replace("ギ", "ぎ");
        kana = kana.Replace("グ", "ぐ");
        kana = kana.Replace("ゲ", "げ");
        kana = kana.Replace("ゴ", "ご");
        kana = kana.Replace("ジ", "じ");
        kana = kana.Replace("ダ", "だ");
        kana = kana.Replace("デ", "で");
        kana = kana.Replace("ド", "ど");
        kana = kana.Replace("バ", "ば");
        kana = kana.Replace("ビ", "び");
        kana = kana.Replace("ブ", "ぶ");
        kana = kana.Replace("ベ", "べ");
        kana = kana.Replace("ボ", "ぼ");
        kana = kana.Replace("パ", "ぱ");
        kana = kana.Replace("ピ", "ぴ");
        kana = kana.Replace("プ", "ぷ");
        kana = kana.Replace("ペ", "ぺ");
        kana = kana.Replace("ポ", "ぽ");
        kana = kana.Replace("ザ", "ざ");
        kana = kana.Replace("ズ", "ず");
        kana = kana.Replace("ゼ", "ぜ");
        kana = kana.Replace("ゾ", "ぞ");
        kana = kana.Replace("ァ", "ぁ");
        kana = kana.Replace("ィ", "ぃ");
        kana = kana.Replace("ゥ", "ぅ");
        kana = kana.Replace("ェ", "ぇ");
        kana = kana.Replace("ォ", "ぉ");
        kana = kana.Replace("ン", "ん");
        kana = kana.Replace("ア", "あ");
        kana = kana.Replace("イ", "い");
        kana = kana.Replace("ウ", "う");
        kana = kana.Replace("エ", "え");
        kana = kana.Replace("オ", "お");

        return kana;
    }

    /// <summary>
    /// Specifically converts a romaji string to kana.
    /// The result may be hiragana, katakana or mixed, depending on the case
    /// of the input romaji.
    /// </summary>
    /// <param name="romaji">Input romaji string.</param>
    /// <param name="isLive">Set to true to specify that the conversion is done in
    /// live (while the user is writing). Disables certain functions.</param>
    /// <returns>Output kana string.</returns>
    public static string RomajiToKana(string romaji, bool isLive = false)
    {
        var options = RegexOptions.CultureInvariant | RegexOptions.Multiline;

        // Replace the double vowels for katakana.
        var doubleVowelRegex = new Regex("([AOIUE])\\1", options);
        romaji = doubleVowelRegex.Replace(romaji, "$1ー");

        // Replace the double consonants.
        var doubleKatakanaConsonnantRegex = new Regex("([BCDFGHJKLMPQRSTVWXZ])\\1", options);
        romaji = doubleKatakanaConsonnantRegex.Replace(romaji, "ッ$1");
        var doubleHiraganaConsonnantRegex = new Regex("([bcdfghjklmpqrstvwxz])\\1", options);
        romaji = doubleHiraganaConsonnantRegex.Replace(romaji, "っ$1");

        // Then, replace - by ー.
        romaji = romaji.Replace('-', 'ー');
        romaji = romaji.Replace('_', 'ー');

        // Then, replace 4 letter characters:
        romaji = romaji.Replace("XTSU", "ッ");
        romaji = romaji.Replace("xtsu", "っ");

        // Then, replace 3 letter characters:
        romaji = romaji.Replace("CHA", "チャ");
        romaji = romaji.Replace("cha", "ちゃ");
        romaji = romaji.Replace("CHI", "チ");
        romaji = romaji.Replace("chi", "ち");
        romaji = romaji.Replace("CHU", "チュ");
        romaji = romaji.Replace("chu", "ちゅ");
        romaji = romaji.Replace("CHE", "チェ");
        romaji = romaji.Replace("che", "ちぇ");
        romaji = romaji.Replace("CHO", "チョ");
        romaji = romaji.Replace("cho", "ちょ");
        romaji = romaji.Replace("SHA", "シャ");
        romaji = romaji.Replace("sha", "しゃ");
        romaji = romaji.Replace("SHI", "シ");
        romaji = romaji.Replace("shi", "し");
        romaji = romaji.Replace("SHU", "シュ");
        romaji = romaji.Replace("shu", "しゅ");
        romaji = romaji.Replace("SHE", "シェ");
        romaji = romaji.Replace("she", "しぇ");
        romaji = romaji.Replace("SHO", "ショ");
        romaji = romaji.Replace("sho", "しょ");
        romaji = romaji.Replace("RYA", "リャ");
        romaji = romaji.Replace("rya", "りゃ");
        romaji = romaji.Replace("RYI", "リィ");
        romaji = romaji.Replace("ryi", "りぃ");
        romaji = romaji.Replace("RYU", "リュ");
        romaji = romaji.Replace("ryu", "りゅ");
        romaji = romaji.Replace("RYE", "リェ");
        romaji = romaji.Replace("rye", "りぇ");
        romaji = romaji.Replace("RYO", "リョ");
        romaji = romaji.Replace("ryo", "りょ");
        romaji = romaji.Replace("HYA", "ヒャ");
        romaji = romaji.Replace("hya", "ひゃ");
        romaji = romaji.Replace("HYI", "ヒィ");
        romaji = romaji.Replace("hyi", "ひぃ");
        romaji = romaji.Replace("HYU", "ヒュ");
        romaji = romaji.Replace("hyu", "ひゅ");
        romaji = romaji.Replace("HYE", "ヒェ");
        romaji = romaji.Replace("hye", "ひぇ");
        romaji = romaji.Replace("HYO", "ヒョ");
        romaji = romaji.Replace("hyo", "ひょ");
        romaji = romaji.Replace("BYA", "ビャ");
        romaji = romaji.Replace("bya", "びゃ");
        romaji = romaji.Replace("BYI", "ビィ");
        romaji = romaji.Replace("byi", "びぃ");
        romaji = romaji.Replace("BYU", "ビュ");
        romaji = romaji.Replace("byu", "びゅ");
        romaji = romaji.Replace("BYE", "ビェ");
        romaji = romaji.Replace("bye", "びぇ");
        romaji = romaji.Replace("BYO", "ビョ");
        romaji = romaji.Replace("byo", "びょ");
        romaji = romaji.Replace("PYA", "ピャ");
        romaji = romaji.Replace("pya", "ぴゃ");
        romaji = romaji.Replace("PYI", "ピィ");
        romaji = romaji.Replace("pyi", "ぴぃ");
        romaji = romaji.Replace("PYU", "ピュ");
        romaji = romaji.Replace("pyu", "ぴゅ");
        romaji = romaji.Replace("PYE", "ピェ");
        romaji = romaji.Replace("pye", "ぴぇ");
        romaji = romaji.Replace("PYO", "ピョ");
        romaji = romaji.Replace("pyo", "ぴょ");
        romaji = romaji.Replace("MYA", "ミャ");
        romaji = romaji.Replace("mya", "みゃ");
        romaji = romaji.Replace("MYI", "ミィ");
        romaji = romaji.Replace("myi", "みぃ");
        romaji = romaji.Replace("MYU", "ミュ");
        romaji = romaji.Replace("myu", "みゅ");
        romaji = romaji.Replace("MYE", "ミェ");
        romaji = romaji.Replace("mye", "みぇ");
        romaji = romaji.Replace("MYO", "ミョ");
        romaji = romaji.Replace("myo", "みょ");
        romaji = romaji.Replace("KYA", "キャ");
        romaji = romaji.Replace("kya", "きゃ");
        romaji = romaji.Replace("KYI", "キィ");
        romaji = romaji.Replace("kyi", "きぃ");
        romaji = romaji.Replace("KYU", "キュ");
        romaji = romaji.Replace("kyu", "きゅ");
        romaji = romaji.Replace("KYE", "キェ");
        romaji = romaji.Replace("kye", "きぇ");
        romaji = romaji.Replace("KYO", "キョ");
        romaji = romaji.Replace("kyo", "きょ");
        romaji = romaji.Replace("GYA", "ギャ");
        romaji = romaji.Replace("gya", "ぎゃ");
        romaji = romaji.Replace("GYI", "ギィ");
        romaji = romaji.Replace("gyi", "ぎぃ");
        romaji = romaji.Replace("GYU", "ギュ");
        romaji = romaji.Replace("gyu", "ぎゅ");
        romaji = romaji.Replace("GYE", "ギェ");
        romaji = romaji.Replace("gye", "ぎぇ");
        romaji = romaji.Replace("GYO", "ギョ");
        romaji = romaji.Replace("gyo", "ぎょ");
        romaji = romaji.Replace("NYA", "ニャ");
        romaji = romaji.Replace("nya", "にゃ");
        romaji = romaji.Replace("NYI", "ニィ");
        romaji = romaji.Replace("nyi", "にぃ");
        romaji = romaji.Replace("NYU", "ニュ");
        romaji = romaji.Replace("nyu", "にゅ");
        romaji = romaji.Replace("NYE", "ニェ");
        romaji = romaji.Replace("nye", "にぇ");
        romaji = romaji.Replace("NYO", "ニョ");
        romaji = romaji.Replace("nyo", "にょ");
        romaji = romaji.Replace("JYA", "ジャ");
        romaji = romaji.Replace("jya", "じゃ");
        romaji = romaji.Replace("JYI", "ジィ");
        romaji = romaji.Replace("jyi", "じぃ");
        romaji = romaji.Replace("JYU", "ジュ");
        romaji = romaji.Replace("jyu", "じゅ");
        romaji = romaji.Replace("JYE", "ジェ");
        romaji = romaji.Replace("jye", "じぇ");
        romaji = romaji.Replace("JYO", "ジョ");
        romaji = romaji.Replace("jyo", "じょ");
        romaji = romaji.Replace("TSU", "ツ");
        romaji = romaji.Replace("tsu", "つ");
        romaji = romaji.Replace("DZU", "ヅ");
        romaji = romaji.Replace("dzu", "づ");
        romaji = romaji.Replace("DZI", "ヂ");
        romaji = romaji.Replace("dzi", "ぢ");
        romaji = romaji.Replace("DYA", "ヂャ");
        romaji = romaji.Replace("dya", "ぢゃ");
        romaji = romaji.Replace("DYI", "ヂィ");
        romaji = romaji.Replace("dyi", "ぢぃ");
        romaji = romaji.Replace("DYU", "ヂュ");
        romaji = romaji.Replace("dyu", "ぢゅ");
        romaji = romaji.Replace("DYE", "ヂェ");
        romaji = romaji.Replace("dye", "ぢぇ");
        romaji = romaji.Replace("DYO", "ヂョ");
        romaji = romaji.Replace("dyo", "ぢょ");
        romaji = romaji.Replace("XWA", "ヮ");
        romaji = romaji.Replace("xwa", "ゎ");
        romaji = romaji.Replace("XKA", "ヵ");
        romaji = romaji.Replace("xka", "ヵ");
        romaji = romaji.Replace("XKE", "ヶ");
        romaji = romaji.Replace("xke", "ヶ");
        romaji = romaji.Replace("XYA", "ャ");
        romaji = romaji.Replace("xya", "ゃ");
        romaji = romaji.Replace("XYI", "ィ");
        romaji = romaji.Replace("xyi", "ぃ");
        romaji = romaji.Replace("XYU", "ュ");
        romaji = romaji.Replace("xyu", "ゅ");
        romaji = romaji.Replace("XYE", "ェ");
        romaji = romaji.Replace("xye", "ぇ");
        romaji = romaji.Replace("XYO", "ョ");
        romaji = romaji.Replace("xyo", "ょ");

        // Then, replace 2 letter characters:
        romaji = romaji.Replace("KA", "カ");
        romaji = romaji.Replace("ka", "か");
        romaji = romaji.Replace("KI", "キ");
        romaji = romaji.Replace("ki", "き");
        romaji = romaji.Replace("KU", "ク");
        romaji = romaji.Replace("ku", "く");
        romaji = romaji.Replace("KE", "ケ");
        romaji = romaji.Replace("ke", "け");
        romaji = romaji.Replace("KO", "コ");
        romaji = romaji.Replace("ko", "こ");
        romaji = romaji.Replace("CA", "カ");
        romaji = romaji.Replace("ca", "か");
        romaji = romaji.Replace("CI", "キ");
        romaji = romaji.Replace("ci", "き");
        romaji = romaji.Replace("CU", "ク");
        romaji = romaji.Replace("cu", "く");
        romaji = romaji.Replace("CE", "ケ");
        romaji = romaji.Replace("ce", "け");
        romaji = romaji.Replace("CO", "コ");
        romaji = romaji.Replace("co", "こ");
        romaji = romaji.Replace("SA", "サ");
        romaji = romaji.Replace("sa", "さ");
        romaji = romaji.Replace("SI", "シ");
        romaji = romaji.Replace("si", "し");
        romaji = romaji.Replace("SU", "ス");
        romaji = romaji.Replace("su", "す");
        romaji = romaji.Replace("SE", "セ");
        romaji = romaji.Replace("se", "せ");
        romaji = romaji.Replace("SO", "ソ");
        romaji = romaji.Replace("so", "そ");
        romaji = romaji.Replace("TA", "タ");
        romaji = romaji.Replace("ta", "た");
        romaji = romaji.Replace("TE", "テ");
        romaji = romaji.Replace("ti", "ち");
        romaji = romaji.Replace("TI", "チ");
        romaji = romaji.Replace("tu", "つ");
        romaji = romaji.Replace("TU", "ツ");
        romaji = romaji.Replace("te", "て");
        romaji = romaji.Replace("TO", "ト");
        romaji = romaji.Replace("to", "と");
        romaji = romaji.Replace("NA", "ナ");
        romaji = romaji.Replace("na", "な");
        romaji = romaji.Replace("NI", "ニ");
        romaji = romaji.Replace("ni", "に");
        romaji = romaji.Replace("NU", "ヌ");
        romaji = romaji.Replace("nu", "ぬ");
        romaji = romaji.Replace("NE", "ネ");
        romaji = romaji.Replace("ne", "ね");
        romaji = romaji.Replace("NO", "ノ");
        romaji = romaji.Replace("no", "の");
        romaji = romaji.Replace("HA", "ハ");
        romaji = romaji.Replace("ha", "は");
        romaji = romaji.Replace("HI", "ヒ");
        romaji = romaji.Replace("hi", "ひ");
        romaji = romaji.Replace("HU", "フ");
        romaji = romaji.Replace("hu", "ふ");
        romaji = romaji.Replace("HE", "ヘ");
        romaji = romaji.Replace("he", "へ");
        romaji = romaji.Replace("HO", "ホ");
        romaji = romaji.Replace("ho", "ほ");
        romaji = romaji.Replace("MA", "マ");
        romaji = romaji.Replace("ma", "ま");
        romaji = romaji.Replace("MI", "ミ");
        romaji = romaji.Replace("mi", "み");
        romaji = romaji.Replace("MU", "ム");
        romaji = romaji.Replace("mu", "む");
        romaji = romaji.Replace("ME", "メ");
        romaji = romaji.Replace("me", "め");
        romaji = romaji.Replace("MO", "モ");
        romaji = romaji.Replace("mo", "も");
        romaji = romaji.Replace("YA", "ヤ");
        romaji = romaji.Replace("ya", "や");
        romaji = romaji.Replace("YU", "ユ");
        romaji = romaji.Replace("yu", "ゆ");
        romaji = romaji.Replace("YE", "イェ");
        romaji = romaji.Replace("ye", "いぇ");
        romaji = romaji.Replace("YO", "ヨ");
        romaji = romaji.Replace("yo", "よ");
        romaji = romaji.Replace("RA", "ラ");
        romaji = romaji.Replace("ra", "ら");
        romaji = romaji.Replace("RI", "リ");
        romaji = romaji.Replace("ri", "り");
        romaji = romaji.Replace("RU", "ル");
        romaji = romaji.Replace("ru", "る");
        romaji = romaji.Replace("RE", "レ");
        romaji = romaji.Replace("re", "れ");
        romaji = romaji.Replace("RO", "ロ");
        romaji = romaji.Replace("ro", "ろ");
        romaji = romaji.Replace("LA", "ラ");
        romaji = romaji.Replace("la", "ら");
        romaji = romaji.Replace("LI", "リ");
        romaji = romaji.Replace("li", "り");
        romaji = romaji.Replace("LU", "ル");
        romaji = romaji.Replace("lu", "る");
        romaji = romaji.Replace("LE", "レ");
        romaji = romaji.Replace("le", "れ");
        romaji = romaji.Replace("LO", "ロ");
        romaji = romaji.Replace("lo", "ろ");
        romaji = romaji.Replace("WA", "ワ");
        romaji = romaji.Replace("wa", "わ");
        romaji = romaji.Replace("WI", "ウィ");
        romaji = romaji.Replace("wi", "うぃ");
        romaji = romaji.Replace("WU", "ウ");
        romaji = romaji.Replace("wu", "う");
        romaji = romaji.Replace("WE", "ウェ");
        romaji = romaji.Replace("we", "うぇ");
        romaji = romaji.Replace("WO", "ヲ");
        romaji = romaji.Replace("wo", "を");
        romaji = romaji.Replace("GA", "ガ");
        romaji = romaji.Replace("ga", "が");
        romaji = romaji.Replace("GI", "ギ");
        romaji = romaji.Replace("gi", "ぎ");
        romaji = romaji.Replace("GU", "グ");
        romaji = romaji.Replace("gu", "ぐ");
        romaji = romaji.Replace("GE", "ゲ");
        romaji = romaji.Replace("ge", "げ");
        romaji = romaji.Replace("GO", "ゴ");
        romaji = romaji.Replace("go", "ご");
        romaji = romaji.Replace("JA", "ジャ");
        romaji = romaji.Replace("ja", "じゃ");
        romaji = romaji.Replace("JI", "ジ");
        romaji = romaji.Replace("ji", "じ");
        romaji = romaji.Replace("JU", "ジュ");
        romaji = romaji.Replace("ju", "じゅ");
        romaji = romaji.Replace("JE", "ジェ");
        romaji = romaji.Replace("je", "じぇ");
        romaji = romaji.Replace("JO", "ジョ");
        romaji = romaji.Replace("jo", "じょ");
        romaji = romaji.Replace("DA", "ダ");
        romaji = romaji.Replace("da", "だ");
        romaji = romaji.Replace("DI", "ヂ");
        romaji = romaji.Replace("di", "ぢ");
        romaji = romaji.Replace("DU", "ヅ");
        romaji = romaji.Replace("du", "づ");
        romaji = romaji.Replace("DE", "デ");
        romaji = romaji.Replace("de", "で");
        romaji = romaji.Replace("DO", "ド");
        romaji = romaji.Replace("do", "ど");
        romaji = romaji.Replace("BA", "バ");
        romaji = romaji.Replace("ba", "ば");
        romaji = romaji.Replace("BI", "ビ");
        romaji = romaji.Replace("bi", "び");
        romaji = romaji.Replace("BU", "ブ");
        romaji = romaji.Replace("bu", "ぶ");
        romaji = romaji.Replace("BE", "ベ");
        romaji = romaji.Replace("be", "べ");
        romaji = romaji.Replace("BO", "ボ");
        romaji = romaji.Replace("bo", "ぼ");
        romaji = romaji.Replace("VA", "ヴァ");
        romaji = romaji.Replace("va", "ヴぁ");
        romaji = romaji.Replace("VI", "ヴィ");
        romaji = romaji.Replace("vi", "ヴぃ");
        romaji = romaji.Replace("VU", "ヴ");
        romaji = romaji.Replace("vu", "ヴ");
        romaji = romaji.Replace("VE", "ヴェ");
        romaji = romaji.Replace("ve", "ヴぇ");
        romaji = romaji.Replace("VO", "ヴォ");
        romaji = romaji.Replace("vo", "ヴぉ");
        romaji = romaji.Replace("PA", "パ");
        romaji = romaji.Replace("pa", "ぱ");
        romaji = romaji.Replace("PI", "ピ");
        romaji = romaji.Replace("pi", "ぴ");
        romaji = romaji.Replace("PU", "プ");
        romaji = romaji.Replace("pu", "ぷ");
        romaji = romaji.Replace("PE", "ペ");
        romaji = romaji.Replace("pe", "ぺ");
        romaji = romaji.Replace("PO", "ポ");
        romaji = romaji.Replace("po", "ぽ");
        romaji = romaji.Replace("ZA", "ザ");
        romaji = romaji.Replace("za", "ざ");
        romaji = romaji.Replace("ZI", "ジ");
        romaji = romaji.Replace("zi", "じ");
        romaji = romaji.Replace("ZU", "ズ");
        romaji = romaji.Replace("zu", "ず");
        romaji = romaji.Replace("ZE", "ゼ");
        romaji = romaji.Replace("ze", "ぜ");
        romaji = romaji.Replace("ZO", "ゾ");
        romaji = romaji.Replace("zo", "ぞ");
        romaji = romaji.Replace("FA", "ファ");
        romaji = romaji.Replace("fa", "ふぁ");
        romaji = romaji.Replace("FI", "フィ");
        romaji = romaji.Replace("fi", "ふぃ");
        romaji = romaji.Replace("FU", "フ");
        romaji = romaji.Replace("fu", "ふ");
        romaji = romaji.Replace("FE", "フェ");
        romaji = romaji.Replace("fe", "ふぇ");
        romaji = romaji.Replace("FO", "フォ");
        romaji = romaji.Replace("fo", "ふぉ");
        romaji = romaji.Replace("XA", "ァ");
        romaji = romaji.Replace("xa", "ぁ");
        romaji = romaji.Replace("XI", "ィ");
        romaji = romaji.Replace("xi", "ぃ");
        romaji = romaji.Replace("XU", "ゥ");
        romaji = romaji.Replace("xu", "ぅ");
        romaji = romaji.Replace("XE", "ェ");
        romaji = romaji.Replace("xe", "ぇ");
        romaji = romaji.Replace("XO", "ォ");
        romaji = romaji.Replace("xo", "ぉ");
        romaji = romaji.Replace("XN", "ン");
        romaji = romaji.Replace("xn", "ん");
        romaji = romaji.Replace("NN", "ン");
        romaji = romaji.Replace("N'", "ン");
        romaji = romaji.Replace("nn", "ん");
        romaji = romaji.Replace("n'", "ん");

        // Then, replace 1 letter characters:
        romaji = romaji.Replace("A", "ア");
        romaji = romaji.Replace("a", "あ");
        romaji = romaji.Replace("I", "イ");
        romaji = romaji.Replace("i", "い");
        romaji = romaji.Replace("U", "ウ");
        romaji = romaji.Replace("u", "う");
        romaji = romaji.Replace("E", "エ");
        romaji = romaji.Replace("e", "え");
        romaji = romaji.Replace("O", "オ");
        romaji = romaji.Replace("o", "お");

        if (!isLive)
        {
            romaji = romaji.Replace("N", "ン");
            romaji = romaji.Replace("n", "ん");
        }

        return romaji;
    }

    #endregion

    #endregion

    #region Check

    /// <summary>
    /// Checks if the given string is written in katakana entirely.
    /// </summary>
    /// <param name="input">String to test.</param>
    /// <returns>True if the input string contains only katakana characters. False otherwise.</returns>
    public static bool IsAllKatakana(string input)
    {
        foreach (char c in input)
        {
            if (!KatakanaDictionary.Contains(c))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the given string is written in kana (hiragana and/or katakana) entirely.
    /// </summary>
    /// <param name="input">String to test.</param>
    /// <returns>True if the input string contains only hiragana and/or katakana characters.
    /// False otherwise.</returns>
    public static bool IsAllKana(string input)
    {
        foreach (char c in input)
        {
            if (!KatakanaDictionary.Contains(c) && !HiraganaDictionary.Contains(c))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Given two kana strings, returns a value indicating if they are equivalent kana
    /// strings.
    /// </summary>
    /// <param name="a">First string to compare.</param>
    /// <param name="b">Second string to compare.</param>
    /// <returns>True if both kana strings are equivalent. False otherwise.</returns>
    public static bool AreEquivalent(string a, string b)
    {
        return ToCommonFormat(a) == ToCommonFormat(b);
    }

    private static string ToCommonFormat(string input)
    {
        return ToHiragana(input.Replace("・", string.Empty));
    }

    #endregion
}
