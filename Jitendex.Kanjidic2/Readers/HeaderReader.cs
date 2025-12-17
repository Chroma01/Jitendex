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
using Jitendex.Kanjidic2.Models;

namespace Jitendex.Kanjidic2.Readers;

internal partial class HeaderReader
{
    private readonly ILogger<EntriesReader> _logger;
    private readonly XmlReader _xmlReader;

    public HeaderReader(ILogger<EntriesReader> logger, XmlReader xmlReader) =>
        (_logger, _xmlReader) =
        (@logger, @xmlReader);

    public async Task<Header> ReadAsync()
    {
        var header = new Header
        {
            DatabaseVersion = null!,
            FileVersion = null!,
            DateOfCreation = null!,
        };

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadElementAsync(header);
                    break;
                case XmlNodeType.DocumentType:
                    // No usable info in here.
                    break;
                case XmlNodeType.Text:
                    var text = await _xmlReader.GetValueAsync();
                    LogUnexpectedTextNode(text);
                    break;
                case XmlNodeType.EndElement:
                    exit = _xmlReader.Name == Header.XmlTagName;
                    break;
            }
        }

        if (header.DatabaseVersion is null)
        {
            LogMissingDatabaseVersion();
            header.DatabaseVersion = Guid.NewGuid().ToString();
        }
        if (header.FileVersion is null)
        {
            LogMissingFileVersion();
            header.FileVersion = Guid.NewGuid().ToString();
        }
        if (header.DateOfCreation is null)
        {
            LogMissingDateOfCreation();
            header.DateOfCreation = Guid.NewGuid().ToString();
        }

        return header;
    }

    private async Task ReadElementAsync(Header header)
    {
        switch (_xmlReader.Name)
        {
            case "kanjidic2":
                break;
            case Header.XmlTagName:
                break;
            case Header.database_XmlTagName:
                header.DatabaseVersion = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            case Header.file_XmlTagName:
                header.FileVersion = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            case Header.date_XmlTagName:
                header.DateOfCreation = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            default:
                LogUnexpectedElement(_xmlReader.Name);
                break;
        }
    }

#pragma warning disable IDE0060

    [LoggerMessage(LogLevel.Warning,
    "Unexpected text node found in document preamble: `{Text}`")]
    partial void LogUnexpectedTextNode(string text);

    [LoggerMessage(LogLevel.Warning,
    "Unexpected element node found in document preamble: <{Name}>")]
    partial void LogUnexpectedElement(string name);

    [LoggerMessage(LogLevel.Warning,
    "No database version found in header")]
    partial void LogMissingDatabaseVersion();

    [LoggerMessage(LogLevel.Warning,
    "No file version found in header")]
    partial void LogMissingFileVersion();

    [LoggerMessage(LogLevel.Warning,
    "No creation date found in header")]
    partial void LogMissingDateOfCreation();

#pragma warning restore IDE0060

}
