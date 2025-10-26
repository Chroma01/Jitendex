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

using static Jitendex.Chise.Readers.ChiseError;

namespace Jitendex.Chise.Readers;

internal class Logger
{
    private readonly Dictionary<ChiseError, List<string>> _logs = [];
    public void LogInvalidUnicodeCodepoint(in LineElements line) => Log(line, InvalidUnicodeCodepoint);
    public void LogUnicodeCharacterInequality(in LineElements line) => Log(line, UnicodeCharacterInequality);
    public void LogInsufficientIdsArgs(in LineElements line) => Log(line, InsufficientIdsArgs);
    public void LogInsufficientIdsOps(in LineElements line) => Log(line, InsufficientIdsOps);
    public void LogInsufficientAltIdsArgs(in LineElements line) => Log(line, InsufficientAltIdsArgs);
    public void LogInsufficientAltIdsOps(in LineElements line) => Log(line, InsufficientAltIdsOps);

    public void LogLineErrors(in LineElements line)
    {
        if (line.InsufficientElementsError)
        {
            Log(line, InsufficientLineElements);
        }
        if (line.ExcessiveElementsError)
        {
            Log(line, ExcessiveLineElements);
        }
        if (line.AltSequenceFormatError)
        {
            Log(line, AltSequenceFormatError);
        }
    }
    private void Log(in LineElements line, ChiseError error)
    {
        if (_logs.TryGetValue(error, out var lines))
        {
            lines.Add(line.ToString());
        }
        else
        {
            _logs[error] = [line.ToString()];
        }
    }

    public void WriteLogs()
    {
        if (_logs.Count == 0)
        {
            return;
        }

        var paths = new LogFilePaths();

        Console.Error.WriteLine($"Log directory: {paths.Directory}");

        foreach (var (error, lines) in _logs)
        {
            var path = paths.GetLogFilePath(error);
            File.WriteAllLines(path.ToString(), lines);
            Console.Error.WriteLine($"\t{Path.GetFileName(path)}\t{lines.Count:n0} logs");
        }
    }
}
