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

namespace Jitendex.Warehouse.Jmdict.Models;

internal class KeywordFactory
{
    private readonly Dictionary<(Type, string), object> Cache = [];

    private readonly Dictionary<(Type, string), string> NameToDescription = [];
    private readonly Dictionary<(Type, string), string> DescriptionToName = [];

    public void Register<T>(string name, string description) where T : IKeyword
    {
        NameToDescription.Add((typeof(T), name), description);
        DescriptionToName.Add((typeof(T), description), name);
    }

    public T GetByName<T>(string name) where T : IKeyword, new()
    {
        var cacheKey = (typeof(T), name);
        if (Cache.TryGetValue(cacheKey, out object? entity))
            return (T)entity;
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
        var newEntity = new T { Name = name, Description = description };
        Cache.Add(cacheKey, newEntity);
        return newEntity;
    }

    public T GetByDescription<T>(string description) where T : IKeyword, new()
    {
        var key = (typeof(T), description);
        if (DescriptionToName.TryGetValue(key, out string? name))
        {
            return GetByName<T>(name);
        }
        throw new ArgumentException
        (
            $"Description `{description}` for type `{typeof(T).Name}` has not been registered.",
            nameof(description)
        );
    }

    #region Corpus

    private readonly Dictionary<CorpusId, Corpus> CorpusCache = [];

    public Corpus GetByCorpusId(CorpusId id)
    {
        if (CorpusCache.TryGetValue(id, out Corpus? corpus))
            return corpus;
        var newCorpus = new Corpus
        { 
            Id = id,
            Name = id.ToString(),
            Description = Corpus.IdToDescription[id],
        };
        CorpusCache.Add(id, newCorpus);
        return newCorpus;
    }

    #endregion
}
