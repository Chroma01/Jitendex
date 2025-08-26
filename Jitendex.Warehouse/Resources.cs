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

using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace Jitendex.Warehouse;

public class Resources
{
    private ILogger<Resources> _logger;

    public Resources(ILogger<Resources> logger)
    {
        _logger = logger;
    }

    public Dictionary<string, T> LoadJsonDictionary<T>(string path)
    {
        Dictionary<string, T> dictionary;
        using (var stream = File.OpenRead(path))
        {
            dictionary = JsonSerializer.Deserialize<Dictionary<string, T>>(stream) ?? [];
        }
        return dictionary;
    }

    public XmlReader CreateXmlReader(string path)
    {
        var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var readerSettings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Parse,
            MaxCharactersFromEntities = long.MaxValue,
            MaxCharactersInDocument = long.MaxValue,
        };
        return XmlReader.Create(fileStream, readerSettings);
    }
}
