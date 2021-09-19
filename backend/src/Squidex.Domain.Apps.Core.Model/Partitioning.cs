// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core
{
    public delegate IFieldPartitioning PartitionResolver(Partitioning key);

    public sealed class Partitioning : IEquatable<Partitioning>
    {
        public static readonly Partitioning Invariant = new Partitioning("invariant");
        public static readonly Partitioning Language = new Partitioning("language");

        public string Key { get; }

        public Partitioning(string key)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            Key = key;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Partitioning);
        }

        public bool Equals(Partitioning? other)
        {
            return string.Equals(other?.Key, Key, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode(StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return Key;
        }

        public static Partitioning FromString(string? value)
        {
            var isLanguage = string.Equals(value, Language.Key, StringComparison.OrdinalIgnoreCase);

            return isLanguage ? Language : Invariant;
        }
    }
}
