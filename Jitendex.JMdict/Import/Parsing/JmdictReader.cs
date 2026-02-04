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
using Jitendex.JMdict.Import.Models;
using Microsoft.Extensions.Logging;

namespace Jitendex.JMdict.Import.Parsing;

internal partial class JmdictReader : BaseReader<JmdictReader>
{
    private readonly DocumentTypeReader _docTypeReader;
    private readonly EntriesReader _entriesReader;

    public JmdictReader(ILogger<JmdictReader> logger, XmlReader xmlReader, DocumentTypeReader docTypeReader, EntriesReader entriesReader) : base(logger, xmlReader)
    {
        _docTypeReader = docTypeReader;
        _entriesReader = entriesReader;
    }

    public async Task<Document> ReadAsync(DateOnly fileDate)
    {
        var document = new Document
        {
            Header = new(fileDate)
        };

        await _docTypeReader.ReadAsync(document);

        var exit = false;
        while (!exit && await _xmlReader.ReadAsync())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    await ReadChildElementAsync(document);
                    break;
                case XmlNodeType.Text:
                    await LogUnexpectedTextNodeAsync(XmlTagName.Root);
                    break;
            }
        }

        return document;
    }

    private async Task ReadChildElementAsync(Document document)
    {
        switch (_xmlReader.Name)
        {
            case XmlTagName.Jmdict:
                await _entriesReader.ReadAsync(document);
                break;
            default:
                LogUnexpectedChildElement(XmlTagName.Root);
                break;
        }
    }
}
