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

using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.JMdict.Models;

namespace Jitendex.JMdict.Readers;

internal partial class EntriesReader
{
    private readonly ILogger<EntriesReader> _logger;
    private readonly XmlReader _xmlReader;
    private readonly EntryReader _entryReader;
    private readonly ReferenceSequencer _referenceSequencer;

    public EntriesReader(ILogger<EntriesReader> logger, XmlReader xmlReader, EntryReader entryReader, ReferenceSequencer referenceSequencer) =>
        (_logger, _xmlReader, _entryReader, _referenceSequencer) =
        (@logger, @xmlReader, @entryReader, @referenceSequencer);

    public async Task<List<Entry>> ReadAsync()
    {
        var entries = new List<Entry>(300_000);

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
                case XmlNodeType.DocumentType:
                    LogUnexpectedDocumentType(_xmlReader.Name);
                    break;
            }
        }

        // Post-processing of all entries.
        await _referenceSequencer.FixCrossReferencesAsync(entries);

        return entries;
    }

    private async Task ReadChildElementAsync(List<Entry> entries)
    {
        switch (_xmlReader.Name)
        {
            case Entry.XmlTagName:
                await _entryReader.ReadAsync(entries);
                break;
            case "JMdict":
                // All of the entries are contained within this element.
                break;
            default:
                LogUnexpectedElement(_xmlReader.Name);
                break;
        }
    }

#pragma warning disable IDE0060

    [LoggerMessage(LogLevel.Warning,
    "Unexpected element node found between entries: <{Name}>")]
    partial void LogUnexpectedElement(string name);

    [LoggerMessage(LogLevel.Warning,
    "Unexpected text node found between entries: `{Text}`")]
    partial void LogUnexpectedTextNode(string text);

    [LoggerMessage(LogLevel.Warning,
    "Unexpected document type node `{Name}`")]
    partial void LogUnexpectedDocumentType(string name);

#pragma warning restore IDE0060
}
