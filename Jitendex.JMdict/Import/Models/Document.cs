/*
Copyright (c) 2025-2026 Stephen Kraus
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

using Jitendex.JMdict.Import.Models.EntryElements;
using Jitendex.JMdict.Import.Models.EntryElements.KanjiFormElements;
using Jitendex.JMdict.Import.Models.EntryElements.ReadingElements;
using Jitendex.JMdict.Import.Models.EntryElements.SenseElements;

namespace Jitendex.JMdict.Import.Models;

internal sealed class Document
{
    public required FileHeader FileHeader { get; init; }
    public Dictionary<int, Entry> Entries { get; init; }

    #region Entry Elements
    public Dictionary<(int, int), KanjiForm> KanjiForms { get; init; }
    public Dictionary<(int, int), Reading> Readings { get; init; }
    public Dictionary<(int, int), Sense> Senses { get; init; }
    #endregion

    #region Kanji Form Elements
    public Dictionary<(int, int, int), KanjiFormInfo> KanjiFormInfos { get; init; }
    public Dictionary<(int, int, int), KanjiFormPriority> KanjiFormPriorities { get; init; }
    #endregion

    #region Reading Elements
    public Dictionary<(int, int, int), ReadingInfo> ReadingInfos { get; init; }
    public Dictionary<(int, int, int), ReadingPriority> ReadingPriorities { get; init; }
    public Dictionary<(int, int, int), Restriction> Restrictions { get; init; }
    #endregion

    #region Sense Elements
    public Dictionary<(int, int, int), CrossReference> CrossReferences { get; init; }
    public Dictionary<(int, int, int), Dialect> Dialects { get; init; }
    public Dictionary<(int, int, int), Field> Fields { get; init; }
    public Dictionary<(int, int, int), Gloss> Glosses { get; init; }
    public Dictionary<(int, int, int), KanjiFormRestriction> KanjiFormRestrictions { get; init; }
    public Dictionary<(int, int, int), LanguageSource> LanguageSources { get; init; }
    public Dictionary<(int, int, int), Misc> Miscs { get; init; }
    public Dictionary<(int, int, int), PartOfSpeech> PartsOfSpeech { get; init; }
    public Dictionary<(int, int, int), ReadingRestriction> ReadingRestrictions { get; init; }
    #endregion

    #region Keywords
    public Dictionary<string, PriorityTag> PriorityTags { get; init; } = [];
    public Dictionary<string, ReadingInfoTag> ReadingInfoTags { get; init; } = [];
    public Dictionary<string, KanjiFormInfoTag> KanjiFormInfoTags { get; init; } = [];
    public Dictionary<string, PartOfSpeechTag> PartOfSpeechTags { get; init; } = [];
    public Dictionary<string, FieldTag> FieldTags { get; init; } = [];
    public Dictionary<string, MiscTag> MiscTags { get; init; } = [];
    public Dictionary<string, DialectTag> DialectTags { get; init; } = [];
    public Dictionary<string, GlossType> GlossTypes { get; init; } = [];
    public Dictionary<string, CrossReferenceType> CrossReferenceTypes { get; init; } = [];
    public Dictionary<string, LanguageSourceType> LanguageSourceTypes { get; init; } = [];
    public Dictionary<string, Language> Languages { get; init; } = [];
    #endregion

    public Dictionary<string, string> KeywordDescriptionToName { get; init; } = [];

    public Document(int expectedEntryCount = 250_000)
    {
        Entries = new(expectedEntryCount);

        KanjiForms = new(expectedEntryCount);
        Readings = new(expectedEntryCount);
        Senses = new(expectedEntryCount);

        KanjiFormInfos = new(expectedEntryCount / 20);
        KanjiFormPriorities = new(expectedEntryCount / 4);

        ReadingInfos = new(expectedEntryCount / 30);
        ReadingPriorities = new(expectedEntryCount / 4);
        Restrictions = new(expectedEntryCount / 25);

        CrossReferences = new(expectedEntryCount / 5);
        Dialects = new(expectedEntryCount / 100);
        Fields = new(expectedEntryCount / 5);
        Glosses = new(expectedEntryCount * 2);
        KanjiFormRestrictions = new(expectedEntryCount / 100);
        LanguageSources = new(expectedEntryCount / 30);
        Miscs = new(expectedEntryCount / 5);
        PartsOfSpeech = new(expectedEntryCount * 2);
        ReadingRestrictions = new(expectedEntryCount / 100);
    }

    public int NextOrder(int key, string typeName) => typeName switch
    {
        nameof(KanjiForm) => NextOrder(key, KanjiForms),
        nameof(Reading) => NextOrder(key, Readings),
        nameof(Sense) => NextOrder(key, Senses),
        _ => throw new ArgumentOutOfRangeException(nameof(typeName))
    };

    public int NextOrder((int, int) key, string typeName) => typeName switch
    {
        nameof(KanjiFormInfo) => NextOrder(key, KanjiFormInfos),
        nameof(KanjiFormPriority) => NextOrder(key, KanjiFormPriorities),
        nameof(ReadingInfo) => NextOrder(key, ReadingInfos),
        nameof(ReadingPriority) => NextOrder(key, ReadingPriorities),
        nameof(Restriction) => NextOrder(key, Restrictions),
        nameof(CrossReference) => NextOrder(key, CrossReferences),
        nameof(Dialect) => NextOrder(key, Dialects),
        nameof(Field) => NextOrder(key, Fields),
        nameof(Gloss) => NextOrder(key, Glosses),
        nameof(KanjiFormRestriction) => NextOrder(key, KanjiFormRestrictions),
        nameof(LanguageSource) => NextOrder(key, LanguageSources),
        nameof(Misc) => NextOrder(key, Miscs),
        nameof(PartOfSpeech) => NextOrder(key, PartsOfSpeech),
        nameof(ReadingRestriction) => NextOrder(key, ReadingRestrictions),
        _ => throw new ArgumentOutOfRangeException(nameof(typeName))
    };

    private int NextOrder<T>(int parentKey, Dictionary<(int, int), T> dic)
    {
        int i = 0;
        while (dic.ContainsKey((parentKey, i)))
        {
            i++;
        }
        return i;
    }

    private int NextOrder<T>((int, int) parentKey, Dictionary<(int, int, int), T> dic)
    {
        int i = 0;
        while (dic.ContainsKey((parentKey.Item1, parentKey.Item2, i)))
        {
            i++;
        }
        return i;
    }

    public IEnumerable<int> ConcatAllEntryIds()
        => Entries.Keys
            .Concat(KanjiForms.Keys.Select(static key => key.Item1))
            .Concat(Readings.Keys.Select(static key => key.Item1))
            .Concat(Senses.Keys.Select(static key => key.Item1))
            .Concat(KanjiFormInfos.Keys.Select(static key => key.Item1))
            .Concat(KanjiFormPriorities.Keys.Select(static key => key.Item1))
            .Concat(ReadingInfos.Keys.Select(static key => key.Item1))
            .Concat(ReadingPriorities.Keys.Select(static key => key.Item1))
            .Concat(Restrictions.Keys.Select(static key => key.Item1))
            .Concat(CrossReferences.Keys.Select(static key => key.Item1))
            .Concat(Dialects.Keys.Select(static key => key.Item1))
            .Concat(Fields.Keys.Select(static key => key.Item1))
            .Concat(Glosses.Keys.Select(static key => key.Item1))
            .Concat(KanjiFormRestrictions.Keys.Select(static key => key.Item1))
            .Concat(LanguageSources.Keys.Select(static key => key.Item1))
            .Concat(Miscs.Keys.Select(static key => key.Item1))
            .Concat(PartsOfSpeech.Keys.Select(static key => key.Item1))
            .Concat(ReadingRestrictions.Keys.Select(static key => key.Item1));
}
