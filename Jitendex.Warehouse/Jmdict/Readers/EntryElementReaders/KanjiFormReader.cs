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
using Jitendex.Warehouse.Jmdict.Models;
using Jitendex.Warehouse.Jmdict.Models.EntryElements;
using Jitendex.Warehouse.Jmdict.Models.EntryElements.KanjiFormElements;

namespace Jitendex.Warehouse.Jmdict.Readers.EntryElementReaders;

internal partial class KanjiFormReader : IJmdictReader<Entry, KanjiForm>
{
    private readonly ILogger<KanjiFormReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly IJmdictReader<KanjiForm, KanjiFormInfo> _infoReader;
    private readonly IJmdictReader<KanjiForm, KanjiFormPriority> _priorityReader;

    public KanjiFormReader(ILogger<KanjiFormReader> logger, XmlReader xmlReader, IJmdictReader<KanjiForm, KanjiFormInfo> infoReader, IJmdictReader<KanjiForm, KanjiFormPriority> priorityReader) =>
        (_logger, _xmlReader, _infoReader, _priorityReader) =
        (@logger, @xmlReader, @infoReader, @priorityReader);

    public async Task<KanjiForm> ReadAsync(Entry entry)
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
                    kanjiForm.IsCorrupt = true;
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == KanjiForm.XmlTagName;
                    break;
            }
        }

        if (kanjiForm.Text == string.Empty)
        {
            LogEmptyTextForm(entry.Id, kanjiForm.Order);
            kanjiForm.IsCorrupt = true;
        }
        return kanjiForm;
    }

    private async Task ReadChildElementAsync(KanjiForm kanjiForm)
    {
        switch (_xmlReader.Name)
        {
            case "keb":
                kanjiForm.Text = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            case KanjiFormInfo.XmlTagName:
                var info = await _infoReader.ReadAsync(kanjiForm);
                kanjiForm.Infos.Add(info);
                break;
            case KanjiFormPriority.XmlTagName:
                var priority = await _priorityReader.ReadAsync(kanjiForm);
                kanjiForm.Priorities.Add(priority);
                break;
            default:
                Log.UnexpectedChildElement(_logger, _xmlReader.Name, KanjiForm.XmlTagName);
                kanjiForm.IsCorrupt = true;
                break;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Entry `{EntryId}` kanji form #{Order} contains no text")]
    private partial void LogEmptyTextForm(int entryId, int order);
}
