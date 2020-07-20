// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.Commands
{
    public static class Is
    {
        public static bool Change(DomainId oldValue, DomainId newValue)
        {
            return !Equals(oldValue, newValue);
        }

        public static bool Change(string? oldValue, string? newValue)
        {
            return !Equals(oldValue, newValue);
        }

        public static bool OptionalChange(bool oldValue, [NotNullWhen(true)] bool? newValue)
        {
            return newValue.HasValue && oldValue != newValue.Value;
        }

        public static bool OptionalChange(string oldValue, [NotNullWhen(true)] string? newValue)
        {
            return !string.IsNullOrWhiteSpace(newValue) && !string.Equals(oldValue, newValue);
        }

        public static bool OptionalChange<T>(ISet<T> oldValue, [NotNullWhen(true)] ISet<T>? newValue)
        {
            return newValue != null && !newValue.SetEquals(oldValue);
        }

        public static bool OptionalChange<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> oldValue, [NotNullWhen(true)] IReadOnlyDictionary<TKey, TValue>? newValue) where TKey : notnull
        {
            return newValue != null && !newValue.EqualsDictionary(oldValue);
        }
    }
}