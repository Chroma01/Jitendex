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

using Jitendex.Warehouse.Jmdict.Models;

namespace Jitendex.Warehouse.Jmdict;

internal class EntityFactory
{
    # region Keyword

    private readonly Dictionary<(Type, string), IKeyword> KeywordCache = [];

    private readonly Dictionary<(Type, string), string> NameToDescription = [];
    private readonly Dictionary<(Type, string), string> DescriptionToName = [];

    public void RegisterKeyword<T>(string name, string description) where T : IKeyword
    {
        NameToDescription.Add((typeof(T), name), description);
        DescriptionToName.Add((typeof(T), description), name);
    }

    public T GetKeywordByName<T>(string name) where T : IKeyword, new()
    {
        var cacheKey = (typeof(T), name);
        if (KeywordCache.TryGetValue(cacheKey, out IKeyword? keyword))
            return (T)keyword;
        string description;
        if (NameToDescription.TryGetValue(cacheKey, out string? value))
        {
            description = value;
        }
        else
        {
            // TODO: Log and warn
            description = string.Empty;
        }
        var newKeyword = new T { Name = name, Description = description };
        KeywordCache.Add(cacheKey, newKeyword);
        return newKeyword;
    }

    public T GetKeywordByDescription<T>(string description) where T : IKeyword, new()
    {
        var key = (typeof(T), description);
        if (DescriptionToName.TryGetValue(key, out string? name))
        {
            return GetKeywordByName<T>(name);
        }
        throw new ArgumentException
        (
            $"Description `{description}` for type `{typeof(T).Name}` has not been registered.",
            nameof(description)
        );
    }

    #endregion

    #region Corpus

    private readonly Dictionary<CorpusId, Corpus> CorpusCache = [];

    public Corpus GetCorpus(CorpusId id)
    {
        if (CorpusCache.TryGetValue(id, out Corpus? corpus))
            return corpus;
        var newCorpus = new Corpus
        { 
            Id = id,
            Name = id.ToString(),
        };
        CorpusCache.Add(id, newCorpus);
        return newCorpus;
    }

    #endregion

    #region Example Source

    private readonly Dictionary<(string, int), ExampleSource> ExampleSourceCache = [];

    public ExampleSource GetExampleSource(string typeName, int originKey)
    {
        var cacheKey = (typeName, originKey);
        if (ExampleSourceCache.TryGetValue(cacheKey, out ExampleSource? exampleSource))
            return exampleSource;
        var newExampleSource = new ExampleSource
        {
            TypeName = typeName,
            OriginKey = originKey,
            Text = string.Empty,
            Translation = string.Empty,
            ExampleSourceType = GetKeywordByName<ExampleSourceType>(typeName),
        };
        ExampleSourceCache.Add(cacheKey, newExampleSource);
        return newExampleSource;
    }

    #endregion
}
