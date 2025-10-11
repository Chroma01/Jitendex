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

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Jitendex.JMdict.Models;

[Table(nameof(ExampleSource))]
[PrimaryKey(nameof(TypeName), nameof(OriginKey))]
public class ExampleSource
{
    public required string TypeName { get; set; }
    public required int OriginKey { get; set; }
    public required string Text { get; set; }
    public required string Translation { get; set; }

    [ForeignKey(nameof(TypeName))]
    public required ExampleSourceType ExampleSourceType { get; set; }

    internal const string XmlTagName = "ex_srce";
    internal const string XmlTagName_Keyword = "ex_text";
    internal const string XmlTagName_Sentence = "ex_sent";
    internal const string XmlTagName_Sentence_Japanese = "jpn";
    internal const string XmlTagName_Sentence_English = "eng";
}
