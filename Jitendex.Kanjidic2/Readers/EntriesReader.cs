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
using Jitendex.Kanjidic2.Models;

namespace Jitendex.Kanjidic2.Readers;

internal partial class EntriesReader
{
    private readonly ILogger<EntriesReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly EntryReader _entryReader;

    public EntriesReader(ILogger<EntriesReader> logger, XmlReader xmlReader, EntryReader entryReader) =>
        (_logger, _xmlReader, _entryReader) =
        (@logger, @xmlReader, @entryReader);

    public async Task<List<Entry>> ReadAsync()
    {
        var entries = new List<Entry>();

        while (await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(entries);
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    LogUnexpectedTextNode(text);
                    break;
            }
        }

        return entries;
    }

    private async Task ReadChildElementAsync(List<Entry> entries)
    {
        switch (_xmlReader.Name)
        {
            case Entry.XmlTagName:
                await _entryReader.ReadAsync(entries);
                break;
            default:
                LogUnexpectedElement(_xmlReader.Name);
                break;
        }
    }

#pragma warning disable IDE0060

    [LoggerMessage(LogLevel.Warning,
    "Unexpected text node found between entries: `{Text}`")]
    partial void LogUnexpectedTextNode(string text);

    [LoggerMessage(LogLevel.Warning,
    "Unexpected element node found between entries: <{Name}>")]
    partial void LogUnexpectedElement(string name);

#pragma warning restore IDE0060

}
