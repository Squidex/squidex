// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.Commands
{
    public static class Is
    {
        public static bool Change<T>(T oldValue, T newValue)
        {
            return !Equals(oldValue, newValue);
        }

        public static bool OptionalChange<T>(T oldValue, [NotNullWhen(true)] T? newValue) where T : struct
        {
            return newValue != null && !Equals(oldValue, newValue.Value);
        }

        public static bool OptionalChange<T>(T oldValue, [NotNullWhen(true)] T? newValue) where T : class
        {
            return newValue != null && !Equals(oldValue, newValue);
        }

        public static bool OptionalChange(string oldValue, [NotNullWhen(true)] string? newValue)
        {
            return !string.IsNullOrWhiteSpace(newValue) && !string.Equals(oldValue, newValue, StringComparison.Ordinal);
        }

        public static bool OptionalSetChange<T>(ISet<T> oldValue, [NotNullWhen(true)] ISet<T>? newValue)
        {
            return newValue != null && !newValue.SetEquals(oldValue);
        }

        public static bool OptionalMapChange<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> oldValue, [NotNullWhen(true)] IReadOnlyDictionary<TKey, TValue>? newValue) where TKey : notnull
        {
            return newValue != null && !newValue.EqualsDictionary(oldValue);
        }
    }
}