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
    public void SetValue(object? value, [CallerMemberName] string? key = null)
    {
        if (key != null)
        {
            this[key] = value;
        }
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
