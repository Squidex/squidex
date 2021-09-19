// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptVars : ScriptContext
    {
        public ContentData? Data
        {
            get => GetValue<ContentData?>();
            set => SetValue(value);
        }

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
}
