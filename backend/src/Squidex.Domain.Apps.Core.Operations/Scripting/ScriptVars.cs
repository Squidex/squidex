// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;

namespace Squidex.Domain.Apps.Core.Scripting;

public class ScriptVars : ScriptContext
{
    public object? this[string key]
    {
        get
        {
            TryGetValue(key, out var result);
            return result;
        }
        set => Set(key, value, true);
    }

    public void SetValue(object? value, [CallerMemberName] string? key = null)
    {
        Set(key, value, true);
    }

    public T GetValue<T>([CallerMemberName] string? key = null)
    {
        if (key != null && TryGetValue(key, out var temp) && temp is T result)
        {
            return result;
        }

        return default!;
    }
}
