// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Squidex.Infrastructure;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Scripting;

public class ScriptVars : IReadOnlyDictionary<string, object?>
{
    private readonly Dictionary<string, object?> values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> lockedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> Keys
    {
        get => values.Keys;
    }

    public IEnumerable<object?> Values
    {
        get => values.Values;
    }

    public int Count
    {
        get => values.Count;
    }

    public object? this[string key]
    {
        get
        {
            TryGetValue(key, out var result);
            return result;
        }
        set
        {
            Set(key, value);
        }
    }

    public void CopyFrom(ScriptVars vars)
    {
        Guard.NotNull(vars);

        foreach (var (key, item) in vars.values)
        {
            if (!values.ContainsKey(key))
            {
                Set(key, item, vars.lockedKeys.Contains(key));
            }
        }
    }

    public void Set(string? key, object? value, bool isReadonly = false)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var finalKey = key.ToCamelCase();

        if (lockedKeys.Contains(finalKey))
        {
            return;
        }

        values[finalKey] = value;

        if (isReadonly)
        {
            lockedKeys.Add(finalKey);
        }
        else
        {
            lockedKeys.Remove(finalKey);
        }
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
    {
        Guard.NotNull(key);

        values.TryGetValue(key, out value);
        return true;
    }

    public bool TryGetValueIfExists<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        Guard.NotNull(key);

        value = default!;

        if (values.TryGetValue(key, out var item) && item is T typed)
        {
            value = typed;
            return true;
        }

        return false;
    }

    public ScriptVars SetInitial(object? value, [CallerMemberName] string? key = null)
    {
        Set(key, value, true);
        return this;
    }

    public T GetValue<T>([CallerMemberName] string? key = null)
    {
        if (key != null && TryGetValue(key, out var temp) && temp is T result)
        {
            return result;
        }

        return default!;
    }

    public bool ContainsKey(string key)
    {
        return values.ContainsKey(key);
    }

    IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator()
    {
        return values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return values.GetEnumerator();
    }
}
