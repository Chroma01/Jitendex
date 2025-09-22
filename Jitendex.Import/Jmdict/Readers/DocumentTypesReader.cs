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

using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Warehouse.Jmdict.Models;

namespace Jitendex.Warehouse.Jmdict.Readers;

internal partial class DocumentTypesReader : IJmdictReader<NoParent, NoChild>
{
    private readonly ILogger<DocumentTypesReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly DocumentTypes _docTypes;

    public DocumentTypesReader(ILogger<DocumentTypesReader> logger, XmlReader xmlReader, DocumentTypes docTypes) =>
        (_logger, _xmlReader, _docTypes) =
        (@logger, @xmlReader, @docTypes);

    public async Task ReadAsync(NoParent noParent)
    {
        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    var dtd = await _xmlReader.GetValueAsync();
                    RegisterKeywords(dtd);
                    exit = true;
                    break;
                case XmlNodeType.Element:
                    LogUnexpectedElement(_xmlReader.Name);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    LogUnexpectedText(text);
                    break;
            }
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Element <{xmlTagName}> encountered before the document type definitions have been parsed.")]
    private partial void LogUnexpectedElement(string xmlTagName);

    [LoggerMessage(LogLevel.Warning,
    "Text node `{Text}` encountered before the document type definitions have been parsed.")]
    private partial void LogUnexpectedText(string text);

    [GeneratedRegex(@"<!ENTITY\s+(.*?)\s+""(.*?)"">", RegexOptions.None)]
    private static partial Regex DtdEntityRegex();

    private static Dictionary<string, string> ParseEntities(string dtd)
    {
        var entityNameToDescription = new Dictionary<string, string>();
        foreach (Match match in DtdEntityRegex().Matches(dtd))
        {
            var name = match.Groups[1].Value;
            var description = match.Groups[2].Value;
            try
            {
                entityNameToDescription.Add(name, description);
            }
            catch (ArgumentException)
            {
                if (entityNameToDescription[name] == description)
                {
                    // Ignore repeated definitions that are exact duplicates.
                }
                else
                {
                    throw;
                }
            }
        }
        return entityNameToDescription;
    }

    private void RegisterKeywords(string dtd)
    {
        // Entities explicitly defined in document header.
        foreach (var (name, description) in ParseEntities(dtd))
        {
            // TODO: categorize parsed entities by type.
            _docTypes.RegisterKeyword<ReadingInfoTag>(name, description);
            _docTypes.RegisterKeyword<KanjiFormInfoTag>(name, description);
            _docTypes.RegisterKeyword<PartOfSpeechTag>(name, description);
            _docTypes.RegisterKeyword<FieldTag>(name, description);
            _docTypes.RegisterKeyword<MiscTag>(name, description);
            _docTypes.RegisterKeyword<DialectTag>(name, description);
        }

        // Entities implicitly defined that cannot be parsed from the document.
        foreach (var (name, description) in GlossTypeEntities)
            _docTypes.RegisterKeyword<GlossType>(name, description);

        foreach (var (name, description) in CrossReferenceTypeEntities)
            _docTypes.RegisterKeyword<CrossReferenceType>(name, description);

        foreach (var (name, description) in LanguageSourceTypeEntities)
            _docTypes.RegisterKeyword<LanguageSourceType>(name, description);

        foreach (var (name, description) in ExampleSourceTypeEntities)
            _docTypes.RegisterKeyword<ExampleSourceType>(name, description);

        foreach (var (name, description) in PriorityTagEntities)
            _docTypes.RegisterKeyword<PriorityTag>(name, description);

        foreach (var (name, description) in LanguageEntities)
            _docTypes.RegisterKeyword<Language>(name, description);
    }

    // Gloss types
    internal static readonly Dictionary<string, string> GlossTypeEntities = new()
    {
        ["tm"] = "trademark",
        ["lit"] = "literal",
        ["fig"] = "figurative",
        ["expl"] = "explanation",
    };

    // Cross reference types
    internal static readonly Dictionary<string, string> CrossReferenceTypeEntities = new()
    {
        ["xref"] = "cross-reference",
        ["ant"] = "antonym",
    };

    // Language source types
    internal static readonly Dictionary<string, string> LanguageSourceTypeEntities = new()
    {
        ["full"] = "Full description of the source word or phrase of the loanword",
        ["part"] = "Partial description of the source word or phrase of the loanword",
    };

    // Example source types
    internal static readonly Dictionary<string, string> ExampleSourceTypeEntities = new()
    {
        ["tat"] = "tatoeba.org",
    };

    // Priority tags
    internal static readonly Dictionary<string, string> PriorityTagEntities =
        Enumerable.Range(1, 2).SelectMany(i => new KeyValuePair<string, string>[] {
            new($"news{i}", $"Ranking in wordfreq file, {i} of 2"),
            new($"ichi{i}", $"Ranking from \"Ichimango goi bunruishuu\", {i} of 2"),
            new($"spec{i}", $"Ranking assigned by JMdict editors, {i} of 2"),
            new($"gai{i}",  $"Common loanwords based on wordfreq file, {i} of 2"),
        }).Concat(
        Enumerable.Range(1, 48).Select(i => new KeyValuePair<string, string>
            ($"nf{i:D2}", $"Ranking in wordfreq file, {i} of 48")
        )).ToDictionary();

    // Languages
    internal static readonly Dictionary<string, string> LanguageEntities = new()
    {
        ["afr"] = "Afrikaans",
        ["ain"] = "Ainu",
        ["alg"] = "Algonquian",
        ["amh"] = "Amharic",
        ["ara"] = "Arabic",
        ["arn"] = "Mapudungun",
        ["bnt"] = "Bantu",
        ["bre"] = "Breton",
        ["bul"] = "Bulgarian",
        ["bur"] = "Burmese",
        ["chi"] = "Chinese",
        ["chn"] = "Chinook Jargon",
        ["cze"] = "Czech",
        ["dan"] = "Danish",
        ["dut"] = "Dutch",
        ["eng"] = "English",
        ["epo"] = "Esperanto",
        ["est"] = "Estonian",
        ["fil"] = "Filipino",
        ["fin"] = "Finnish",
        ["fre"] = "French",
        ["geo"] = "Georgian",
        ["ger"] = "German",
        ["glg"] = "Galician",
        ["grc"] = "Ancient Greek",
        ["gre"] = "Modern Greek",
        ["haw"] = "Hawaiian",
        ["heb"] = "Hebrew",
        ["hin"] = "Hindi",
        ["hun"] = "Hungarian",
        ["ice"] = "Icelandic",
        ["ind"] = "Indonesian",
        ["ita"] = "Italian",
        ["khm"] = "Khmer",
        ["kor"] = "Korean",
        ["kur"] = "Kurdish",
        ["lat"] = "Latin",
        ["lit"] = "Lithuanian",
        ["mal"] = "Malayalam",
        ["mao"] = "Maori",
        ["may"] = "Malay",
        ["mnc"] = "Manchu",
        ["mol"] = "Moldavian",
        ["mon"] = "Mongolian",
        ["nor"] = "Norwegian",
        ["per"] = "Persian",
        ["pol"] = "Polish",
        ["por"] = "Portuguese",
        ["rum"] = "Romanian",
        ["rus"] = "Russian",
        ["san"] = "Sanskrit",
        ["scr"] = "Croatian",
        ["slo"] = "Slovak",
        ["slv"] = "Slovenian",
        ["som"] = "Somali",
        ["spa"] = "Spanish",
        ["swa"] = "Swahili",
        ["swe"] = "Swedish",
        ["tah"] = "Tahitian",
        ["tam"] = "Tamil",
        ["tgl"] = "Tagalog",
        ["tha"] = "Thai",
        ["tib"] = "Tibetan",
        ["tur"] = "Turkish",
        ["ukr"] = "Ukrainian",
        ["urd"] = "Urdu",
        ["uzb"] = "Uzbek",
        ["vie"] = "Vietnamese",
        ["yid"] = "Yiddish"
    };
}
