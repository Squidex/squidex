// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core;

public delegate IFieldPartitioning PartitionResolver(Partitioning key);

public sealed record Partitioning
{
    public static readonly Partitioning Invariant = new Partitioning("invariant");
    public static readonly Partitioning Language = new Partitioning("language");

    public string Key { get; }

    public Partitioning(string key)
    {
        Guard.NotNullOrEmpty(key);

        Key = key;
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
