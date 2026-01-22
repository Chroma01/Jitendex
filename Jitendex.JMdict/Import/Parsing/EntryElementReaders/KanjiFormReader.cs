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

using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.JMdict.Import.Models;
using Jitendex.JMdict.Import.Models.EntryElements;
using Jitendex.JMdict.Import.Models.EntryElements.KanjiFormElements;
using Jitendex.JMdict.Import.Parsing.EntryElementReaders.KanjiFormElementReaders;

namespace Jitendex.JMdict.Import.Parsing.EntryElementReaders;

internal partial class KanjiFormReader : BaseReader<KanjiFormReader>
{
    private readonly KInfoReader _infoReader;
    private readonly KPriorityReader _priorityReader;

    public KanjiFormReader(ILogger<KanjiFormReader> logger, XmlReader xmlReader, KInfoReader infoReader, KPriorityReader priorityReader) : base(logger, xmlReader) =>
        (_infoReader, _priorityReader) =
        (@infoReader, @priorityReader);

    public async Task ReadAsync(Document document, Entry entry)
    {
        var kanjiForm = new KanjiForm
        {
            EntryId = entry.Id,
            Order = document.NextOrder(entry.Id, nameof(KanjiForm)),
            Text = null!,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document, kanjiForm);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    UnexpectedTextNode(KanjiForm.XmlTagName, text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == KanjiForm.XmlTagName;
                    break;
            }
        }

        if (kanjiForm.Text is not null)
        {
            document.KanjiForms.Add(kanjiForm.Key(), kanjiForm);
        }
        else
        {
            LogMissingElement(kanjiForm.EntryId, kanjiForm.Order, KanjiForm.Text_XmlTagName);
        }
    }

    private async Task ReadChildElementAsync(Document document, KanjiForm kanjiForm)
    {
        switch (_xmlReader.Name)
        {
            case KanjiForm.Text_XmlTagName:
                await ReadKanjiFormText(kanjiForm);
                break;
            case KanjiFormInfo.XmlTagName:
                await _infoReader.ReadAsync(document, kanjiForm);
                break;
            case KanjiFormPriority.XmlTagName:
                await _priorityReader.ReadAsync(document, kanjiForm);
                break;
            default:
                UnexpectedChildElement(_xmlReader.Name, KanjiForm.XmlTagName);
                break;
        }
    }

    private async Task ReadKanjiFormText(KanjiForm kanjiForm)
    {
        if (kanjiForm.Text is not null)
        {
            LogMultipleElements(kanjiForm.EntryId, kanjiForm.Order, KanjiForm.Text_XmlTagName);
        }

        kanjiForm.Text = await _xmlReader.ReadElementContentAsStringAsync();

        if (string.IsNullOrWhiteSpace(kanjiForm.Text))
        {
            LogEmptyTextForm(kanjiForm.EntryId, kanjiForm.Order);
        }
    }

    [LoggerMessage(LogLevel.Error,
    "Entry `{EntryId}` kanji form #{Order} does not contain a <{XmlTagName}> element")]
    partial void LogMissingElement(int entryId, int order, string xmlTagName);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` kanji form #{Order} contains no text")]
    partial void LogEmptyTextForm(int entryId, int order);

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` kanji form #{Order} contains multiple <{XmlTagName}> elements")]
    partial void LogMultipleElements(int entryId, int order, string xmlTagName);
}
