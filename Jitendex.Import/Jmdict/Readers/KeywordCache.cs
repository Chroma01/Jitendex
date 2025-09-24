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

using Microsoft.Extensions.Logging;

namespace Jitendex.Import.Jmdict.Readers;

internal partial class KeywordCache(ILogger<KeywordCache> logger)
{
    private readonly Dictionary<(Type, string), IKeyword> _cache = [];
    private readonly Dictionary<(Type, string), string> _nameToDescription = [];
    private readonly Dictionary<(Type, string), string> _descriptionToName = [];

    public void Register<T>(string name, string description) where T : IKeyword
    {
        _nameToDescription.Add((typeof(T), name), description);
        _descriptionToName.Add((typeof(T), description), name);
    }

    public T GetByName<T>(string name) where T : IKeyword, new()
    {
        var cacheKey = (typeof(T), name);
        if (_cache.TryGetValue(cacheKey, out IKeyword? keyword))
        {
            return (T)keyword;
        }

        T newKeyword;

        if (_nameToDescription.TryGetValue(cacheKey, out string? value))
        {
            newKeyword = new T { Name = name, Description = value };
        }
        else
        {
            LogUnregisteredKeywordName(name, typeof(T).Name);
            newKeyword = new T { Name = name, Description = string.Empty, IsCorrupt = true };
        }

        _cache.Add(cacheKey, newKeyword);
        return newKeyword;
    }

    public T GetByDescription<T>(string description) where T : IKeyword, new()
    {
        var key = (typeof(T), description);
        if (_descriptionToName.TryGetValue(key, out string? name))
        {
            return GetByName<T>(name);
        }
        else
        {
            LogUnregisteredKeywordDescription(description, typeof(T).Name);
            var impromptuName = Guid.NewGuid().ToString();
            Register<T>(impromptuName, description);
            var keyword = GetByName<T>(impromptuName);
            keyword.IsCorrupt = true;
            return keyword;
        }
    }

    [LoggerMessage(LogLevel.Warning,
    "Keyword name `{Name}` for type `{TypeName}` was not registered with a description before use.")]
    private partial void LogUnregisteredKeywordName(string name, string typeName);

    [LoggerMessage(LogLevel.Warning,
    "Description `{Description}` for type `{TypeName}` was not registered with a keyword name before use.")]
    private partial void LogUnregisteredKeywordDescription(string description, string typeName);
}
