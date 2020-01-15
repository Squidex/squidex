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
        public static bool Change(Guid oldValue, Guid newValue)
        {
            return !Equals(oldValue, newValue);
        }

        public static bool Change(string? oldValue, string newValue)
        {
            return !Equals(oldValue, newValue);
        }

        public static bool ChangeWhenDefined(string? oldValue, string? newValue)
        {
            return !string.IsNullOrWhiteSpace(newValue) && !string.Equals(oldValue, newValue);
        }

        public static bool Change<T>(ISet<T>? oldValue, [NotNullWhen(true)] ISet<T>? newValue)
        {
            return newValue != null && (oldValue == null || !newValue.SetEquals(oldValue));
        }

        public static bool Change<TKey, TValue>(IReadOnlyDictionary<TKey, TValue>? oldValue, [NotNullWhen(true)] IReadOnlyDictionary<TKey, TValue>? newValue) where TKey : notnull
        {
            return newValue != null && (oldValue == null || !newValue.EqualsDictionary(oldValue));
        }
    }
}