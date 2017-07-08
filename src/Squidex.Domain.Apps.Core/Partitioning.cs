// ==========================================================================
//  Partitioning.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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

        public override bool Equals(object obj)
        {
            return Equals(obj as Partitioning);
        }

        public bool Equals(Partitioning other)
        {
            return string.Equals(other?.Key, Key, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
