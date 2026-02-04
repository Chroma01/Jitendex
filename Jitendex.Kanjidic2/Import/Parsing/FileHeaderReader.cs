/*
Copyright (c) 2025-2026 Stephen Kraus
SPDX-License-Identifier: AGPL-3.0-or-later

This file is part of Jitendex.

Jitendex is free software: you can redistribute it and/or modify it under the terms of
the GNU Affero General Public License as published by the Free Software Foundation,
either version 3 of the License or (at your option) any later version.

Jitendex is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with Jitendex.
If not, see <https://www.gnu.org/licenses/>.
*/

using System.Xml;
using Microsoft.Extensions.Logging;
using Jitendex.Kanjidic2.Import.Models;

namespace Jitendex.Kanjidic2.Import.Parsing;

internal partial class HeaderReader : BaseReader<HeaderReader>
{
    public HeaderReader(ILogger<HeaderReader> logger, XmlReader xmlReader) : base(logger, xmlReader) { }

    public async Task<DocumentHeader> ReadAsync()
    {
        var header = new DocumentHeader
        {
            DatabaseVersion = null!,
            FileVersion = null!,
            Date = default,
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
                    exit = _xmlReader.Name == DocumentHeader.XmlTagName;
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
        if (header.Date.Equals(default))
        {
            LogMissingDateOfCreation();
        }

        return header;
    }

    private async Task ReadElementAsync(DocumentHeader header)
    {
        switch (_xmlReader.Name)
        {
            case "kanjidic2":
                break;
            case DocumentHeader.XmlTagName:
                break;
            case DocumentHeader.database_XmlTagName:
                header.DatabaseVersion = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            case DocumentHeader.file_XmlTagName:
                header.FileVersion = await _xmlReader.ReadElementContentAsStringAsync();
                break;
            case DocumentHeader.date_XmlTagName:
                var content = await _xmlReader.ReadElementContentAsStringAsync();
                if (DateOnly.TryParse(content, out var date))
                {
                    header.Date = date;
                }
                else
                {
                    LogUnparsableDate(content);
                }
                break;
            default:
                LogUnexpectedElement(_xmlReader.Name);
                break;
        }
    }

    [LoggerMessage(LogLevel.Warning, "Unexpected text node found in document preamble: `{Text}`")]
    partial void LogUnexpectedTextNode(string text);

    [LoggerMessage(LogLevel.Warning, "Unexpected element node found in document preamble: <{Name}>")]
    partial void LogUnexpectedElement(string name);

    [LoggerMessage(LogLevel.Warning, "No database version found in header")]
    partial void LogMissingDatabaseVersion();

    [LoggerMessage(LogLevel.Warning, "No file version found in header")]
    partial void LogMissingFileVersion();

    [LoggerMessage(LogLevel.Warning, "No creation date found in header")]
    partial void LogMissingDateOfCreation();

    [LoggerMessage(LogLevel.Warning, "Cannot parse creation date: <{Date}>")]
    partial void LogUnparsableDate(string date);
}
