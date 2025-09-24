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

using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Import.Jmdict.Models;
using Jitendex.Import.Jmdict.Models.EntryElements;
using Jitendex.Import.Jmdict.Models.EntryElements.KanjiFormElements;
using Jitendex.Import.Jmdict.Readers.EntryElementReaders.KanjiFormElementReaders;

namespace Jitendex.Import.Jmdict.Readers.EntryElementReaders;

internal partial class KanjiFormReader
{
    private readonly ILogger<KanjiFormReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly KInfoReader _infoReader;
    private readonly KPriorityReader _priorityReader;

    public KanjiFormReader(ILogger<KanjiFormReader> logger, XmlReader xmlReader, KInfoReader infoReader, KPriorityReader priorityReader) =>
        (_logger, _xmlReader, _infoReader, _priorityReader) =
        (@logger, @xmlReader, @infoReader, @priorityReader);

    public async Task ReadAsync(Entry entry)
    {
        var kanjiForm = new KanjiForm
        {
            EntryId = entry.Id,
            Order = entry.KanjiForms.Count + 1,
            Text = string.Empty,
            Entry = entry,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(kanjiForm);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    Log.UnexpectedTextNode(_logger, KanjiForm.XmlTagName, text);
                    kanjiForm.Entry.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == KanjiForm.XmlTagName;
                    break;
            }
        }

        entry.KanjiForms.Add(kanjiForm);
    }

    private async Task ReadChildElementAsync(KanjiForm kanjiForm)
    {
        switch (_xmlReader.Name)
        {
            case KanjiForm.Text_XmlTagName:
                await ReadKanjiFormText(kanjiForm);
                break;
            case KanjiFormInfo.XmlTagName:
                await _infoReader.ReadAsync(kanjiForm);
                break;
            case KanjiFormPriority.XmlTagName:
                await _priorityReader.ReadAsync(kanjiForm);
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, KanjiForm.XmlTagName);
                kanjiForm.Entry.IsCorrupt = true;
                break;
        }
    }

    private async Task ReadKanjiFormText(KanjiForm kanjiForm)
    {
        kanjiForm.Text = await _xmlReader.ReadElementContentAsStringAsync();
        if (string.IsNullOrWhiteSpace(kanjiForm.Text))
        {
            LogEmptyTextForm(kanjiForm.Entry.Id, kanjiForm.Order);
            kanjiForm.Entry.IsCorrupt = true;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` kanji form #{Order} contains no text")]
    private partial void LogEmptyTextForm(int entryId, int order);
}
