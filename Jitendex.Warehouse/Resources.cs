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

namespace Jitendex.Warehouse;

public class Resources
{
    public string JmdictPath { get; set; } = Path.Combine("Resources", "edrdg", "JMdict_e_examp");
    public string Kanjidic2Path { get; set; } = Path.Combine("Resources", "edrdg", "kanjidic2.xml");

    public string JmdictCrossReferenceSequencesPath { get; set; } =
        Path.Combine("Resources", "jmdict", "cross_reference_sequences.json");

    public async Task<Dictionary<string, int>> JmdictCrossReferenceSequencesAsync()
    {
        Dictionary<string, int> cachedSequences;
        await using (var stream = File.OpenRead(JmdictCrossReferenceSequencesPath))
        {
            cachedSequences = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(stream) ?? [];
        }
        return cachedSequences;
    }
}
