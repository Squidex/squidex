// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Squidex.Infrastructure;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Scripting;

public class ScriptContext : IEnumerable<KeyValuePair<string, (object? Value, bool IsReadonly)>>
{
    private readonly Dictionary<string, (object? Value, bool IsReadonly)> values = new Dictionary<string, (object? Value, bool IsReadonly)>(StringComparer.OrdinalIgnoreCase);

    public void CopyFrom(ScriptVars vars)
    {
        Guard.NotNull(vars);

        foreach (var (key, item) in vars)
        {
            if (!values.ContainsKey(key))
            {
                SetItem(key, item);
            }
        }
    }

    public void SetItem(string? key, (object? Value, bool IsReadonly) item)
    {
        Set(key, item.Value, item.IsReadonly);
    }

    public void Set(string? key, object? value, bool isReadonly = false)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var finalKey = key.ToCamelCase();

        if (values.TryGetValue(finalKey, out var existing) && existing.IsReadonly)
        {
            return;
        }

        values[finalKey] = (value, isReadonly);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
    {
        Guard.NotNull(key);

        value = default!;

        if (values.TryGetValue(key, out var item))
        {
            value = item.Value;
            return true;
        }

        return false;
    }

    public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        Guard.NotNull(key);

        value = default!;

        if (values.TryGetValue(key, out var item) && item.Value is T typed)
        {
            value = typed;
            return true;
        }

        return false;
    }

    public IEnumerator<KeyValuePair<string, (object? Value, bool IsReadonly)>> GetEnumerator()
    {
        return values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return values.GetEnumerator();
    }
}
