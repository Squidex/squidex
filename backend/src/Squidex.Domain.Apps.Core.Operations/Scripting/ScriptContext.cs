// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public class ScriptContext : Dictionary<string, object?>
{
    public ScriptContext()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        Guard.NotNull(key);

        value = default!;

        if (TryGetValue(key, out var temp) && temp is T typed)
        {
            value = typed;
            return true;
        }

        return false;
    }
}
