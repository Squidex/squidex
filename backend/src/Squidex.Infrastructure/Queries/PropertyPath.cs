// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Infrastructure.Queries;

public sealed class PropertyPath : ReadonlyList<string>
{
    private static readonly char[] Separators = { '.', '/' };

    public PropertyPath(IList<string> items)
        : base(items)
    {
        if (items.Count == 0)
        {
            ThrowHelper.ArgumentException("Path cannot be empty.", nameof(items));
        }
    }

    public static implicit operator PropertyPath(string path)
    {
        var result = new List<string>();

        var currentPath = path.AsSpan();
        var currentPosition = 0;

        void Add(ReadOnlySpan<char> value)
        {
            var property = value.Trim(Separators).ToString();

            if (property.Length == 0)
            {
                return;
            }

            property = property.Replace("\\/", "/", StringComparison.OrdinalIgnoreCase);
            property = property.Replace("\\.", ".", StringComparison.OrdinalIgnoreCase);

            result.Add(property);
        }

        while (true)
        {
            var nextDot = currentPath[currentPosition..].IndexOfAny(Separators) + currentPosition;

            if (nextDot < currentPosition)
            {
                Add(currentPath);
                break;
            }
            else if (nextDot == currentPosition)
            {
                currentPath = currentPath[1..];
            }
            else if (currentPath[nextDot - 1] == '\\')
            {
                currentPosition = nextDot + 1;
            }
            else
            {
                Add(currentPath[..nextDot]);

                currentPath = currentPath[nextDot..].Trim(Separators);
                currentPosition = 0;
            }
        }

        return Create(result);
    }

    public static implicit operator PropertyPath(string[] path)
    {
        return Create(path);
    }

    public static implicit operator PropertyPath(List<string> path)
    {
        return Create(path);
    }

    public override string ToString()
    {
        return string.Join(".", this);
    }

    private static PropertyPath Create(IEnumerable<string>? source)
    {
        var inner = source?.ToList();

        if (inner == null || inner.Count == 0)
        {
            ThrowHelper.ArgumentException("Path cannot be empty.", nameof(source));
            return null!;
        }
        else
        {
            return new PropertyPath(inner);
        }
    }
}
