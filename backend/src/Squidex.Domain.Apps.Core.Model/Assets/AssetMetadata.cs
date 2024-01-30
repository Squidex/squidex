// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Assets;

public sealed class AssetMetadata : Dictionary<string, JsonValue>
{
    private static readonly char[] PathSeparators = ['.', '[', ']'];

    public int? GetInt32(string name)
    {
        if (TryGetValue(name, out var value) && value.Value is double n)
        {
            return (int)n;
        }

        return null;
    }

    public float? GetSingle(string name)
    {
        if (TryGetValue(name, out var value) && value.Value is double n)
        {
            return (float)n;
        }

        return null;
    }

    public bool TryGetNumber(string name, out double result)
    {
        if (TryGetValue(name, out var value) && value.Value is double n)
        {
            result = n;

            return true;
        }

        result = 0;

        return false;
    }

    public bool TryGetString(string name, [MaybeNullWhen(false)] out string result)
    {
        if (TryGetValue(name, out var value) && value.Value is string s)
        {
            result = s;

            return true;
        }

        result = null!;

        return false;
    }

    public bool TryGetByPath(string? path, [MaybeNullWhen(false)] out object result)
    {
        return TryGetByPath(path?.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries), out result!);
    }

    public bool TryGetByPath(IEnumerable<string>? path, [MaybeNullWhen(false)] out object result)
    {
        result = this;

        if (path == null || !path.Any())
        {
            return false;
        }

        result = null!;

        if (!TryGetValue(path.First(), out var json))
        {
            return false;
        }

        json.TryGetByPath(path.Skip(1), out var temp);

        result = temp!;

        return true;
    }
}
