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
using Jitendex.Import.KanjiVG.Models;

namespace Jitendex.Import.KanjiVG.Readers;

internal partial class StrokeNumberReader
{
    private readonly ILogger<StrokeNumberReader> _logger;
    private readonly StrokeReader _strokeReader;

    public StrokeNumberReader(ILogger<StrokeNumberReader> logger, StrokeReader strokeReader)
    {
        _logger = logger;
        _strokeReader = strokeReader;
    }

    public async Task ReadAsync(XmlReader xmlReader, StrokeNumberGroup group)
    {
        var strokeNumber = new StrokeNumber
        {
            UnicodeScalarValue = group.Entry.UnicodeScalarValue,
            VariantTypeName = group.Entry.VariantTypeName,
            GroupId = group.Id,
            Transform = xmlReader.GetAttribute("transform"),
            Number = await GetNumberAsync(xmlReader),
            Group = group,
        };

        int order = group.StrokeNumbers.Count + 1;
        if (strokeNumber.Number != order)
        {
            LogNumberOutOfOrder(group.Entry.FileName(), strokeNumber.Number, order);
        }

        group.StrokeNumbers.Add(strokeNumber);
    }

    private async Task<int> GetNumberAsync(XmlReader xmlReader)
    {
        var text = await xmlReader.ReadElementContentAsStringAsync();
        if (int.TryParse(text, out int value))
        {
            return value;
        }
        else
        {
            LogUnparsableStrokeNumber(text);
            return -1;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Stroke number text `{Text}` is not an integer")]
    private partial void LogUnparsableStrokeNumber(string text);

    [LoggerMessage(LogLevel.Warning,
    "In file `{FileName}`, stroke number `{Number}` is not equal to its order `{Order}`")]
    private partial void LogNumberOutOfOrder(string fileName, int number, int order);
}
