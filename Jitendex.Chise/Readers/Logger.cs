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

namespace Jitendex.Chise.Readers;

internal class Logger
{
    private readonly string _invalidUnicodeCodepointPath;
    private readonly string _unicodeCharacterInequalityPath;
    private readonly string _insufficientLineElementsPath;
    private readonly string _excessiveLineElementsPath;
    private readonly string _altSequenceFormatErrorPath;
    private readonly string _insufficientIdsArgsPath;
    private readonly string _insufficientIdsOpsPath;
    private readonly string _insufficientAltIdsArgsPath;
    private readonly string _insufficientAltIdsOpsPath;

    public Logger()
    {
        var logDirectory = new DirectoryInfo(Path.Join
        (
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Jitendex",
            "chise-ids-errors"
        ));

        if (logDirectory.Exists)
        {
            foreach (var file in logDirectory.EnumerateFiles())
            {
                file.Delete();
            }
        }
        else
        {
            logDirectory.Create();
        }

        string makePath(string filename) => Path.Join(logDirectory.FullName, filename);

        _invalidUnicodeCodepointPath = makePath("invalid_unicode_codepoint.tsv");
        _unicodeCharacterInequalityPath = makePath("unicode_character_inequality.tsv");
        _insufficientLineElementsPath = makePath("insufficient_line_elements.tsv");
        _excessiveLineElementsPath = makePath("excessive_line_elements.tsv");
        _altSequenceFormatErrorPath = makePath("alt_sequence_format_error.tsv");
        _insufficientIdsArgsPath = makePath("insufficient_ids_args.tsv");
        _insufficientIdsOpsPath = makePath("insufficient_ids_ops.tsv");
        _insufficientAltIdsArgsPath = makePath("insufficient_alt_ids_args.tsv");
        _insufficientAltIdsOpsPath = makePath("insufficient_alt_idc_ops.tsv");
    }

    public void InvalidUnicodeCodepoint(in LineElements line) => Write(line, _invalidUnicodeCodepointPath);
    public void UnicodeCharacterInequality(in LineElements line) => Write(line, _unicodeCharacterInequalityPath);
    public void InsufficientLineElements(in LineElements line) => Write(line, _insufficientLineElementsPath);
    public void ExcessiveLineElements(in LineElements line) => Write(line, _excessiveLineElementsPath);
    public void AltSequenceFormatError(in LineElements line) => Write(line, _altSequenceFormatErrorPath);
    public void InsufficientIdsArgs(in LineElements line) => Write(line, _insufficientIdsArgsPath);
    public void InsufficientIdsOps(in LineElements line) => Write(line, _insufficientIdsOpsPath);
    public void InsufficientAltIdsArgs(in LineElements line) => Write(line, _insufficientAltIdsArgsPath);
    public void InsufficientAltIdsOps(in LineElements line) => Write(line, _insufficientAltIdsOpsPath);

    private static void Write(in LineElements line, string path)
    {
        using var sw = new StreamWriter(path, append: true);
        sw.WriteLine(line.ToString());
    }
}
