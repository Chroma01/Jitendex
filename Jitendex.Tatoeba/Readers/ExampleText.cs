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

namespace Jitendex.Tatoeba.Readers;

internal readonly ref struct ExampleText
{
    private readonly ReadOnlySpan<char> _lineA;
    private readonly ReadOnlySpan<char> _lineB;
    private readonly int _tabIdx;
    private readonly int _idIdx;
    private readonly int _underscoreIdx;

    public ExampleText(ReadOnlySpan<char> lineA, ReadOnlySpan<char> lineB)
    {
        _lineA = lineA;
        _lineB = lineB;

        _tabIdx = _lineA.IndexOf('\t');
        _idIdx = _lineA.IndexOf("#ID=", StringComparison.Ordinal);
        _underscoreIdx = _idIdx > -1
            ? _idIdx + _lineA[_idIdx..].IndexOf("_")
            : -1;
    }

    public string GetJapaneseSentenceText()
    {
        if (_tabIdx == -1)
        {
            throw new InvalidOperationException($"Missing tab character in `{_lineA}`");
        }
        return _lineA[.._tabIdx].ToString();
    }

    public string GetEnglishSentenceText()
    {
        if (_tabIdx == -1)
        {
            throw new InvalidOperationException($"Missing tab character in `{_lineA}`");
        }
        if (_idIdx == -1)
        {
            throw new InvalidOperationException($"Missing ID string in `{_lineA}`");
        }
        return _lineA[(_tabIdx + 1).._idIdx].ToString();
    }

    public int GetEnglishSentenceId()
    {
        if (_idIdx == -1)
        {
            throw new InvalidOperationException($"Missing ID string in `{_lineA}`");
        }
        var text = _lineA[(_idIdx + 4).._underscoreIdx];
        return int.Parse(text);
    }

    public int GetJapaneseSentenceId()
    {
        if (_underscoreIdx == -1)
        {
            throw new InvalidOperationException($"Missing underscore in `{_lineA}`");
        }
        var text = _lineA[(_underscoreIdx + 1)..];
        return int.Parse(text);
    }

    public List<Range> ElementTextRanges()
    {
        List<Range> ranges = [];
        foreach (var range in _lineB.Split(' '))
        {
            ranges.Add(range);
        }
        return ranges;
    }

    public IndexElementText GetElementText(Range range)
    {
        return new IndexElementText(_lineB[range]);
    }
}
