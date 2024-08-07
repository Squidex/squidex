﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.Reflection;

public sealed class TypeConfig
{
    private readonly List<(Type DerivedType, string TypeName)> derivedTypes = [];
    private Dictionary<string, Type>? mapByName;
    private Dictionary<Type, string>? mapByType;

    public string? DiscriminatorProperty { get; set; }

    public bool IsEmpty
    {
        get => derivedTypes.Count == 0;
    }

    internal void Add(Type derivedType, string typeName)
    {
        Guard.NotNull(derivedType);
        Guard.NotNullOrEmpty(typeName);

        if (!derivedTypes.Contains((derivedType, typeName)))
        {
            derivedTypes.Add((derivedType, typeName));
        }

        var (conflict, _) = derivedTypes.Find(x => x.TypeName == typeName && x.DerivedType != derivedType);

        if (conflict != null)
        {
            ThrowHelper.ArgumentException($"Type name '{typeName}' is already used by type '{conflict}", nameof(typeName));
        }

        mapByName = null;
        mapByType = null;
    }

    public IEnumerable<(Type DerivedType, string TypeName)> DerivedTypes()
    {
        var map = mapByType ??= BuildMapByType();

        // The mapping can have multiple names per type, but the last is the default.
        return map.Select(x => (x.Key, x.Value));
    }

    public bool TryGetType(string typeName, [MaybeNullWhen(false)] out Type derivedType)
    {
        var map = mapByName ??= BuildMapByName();

        return map.TryGetValue(typeName, out derivedType);
    }

    public bool TryGetName(Type derivedType, [MaybeNullWhen(false)] out string typeName)
    {
        var map = mapByType ??= BuildMapByType();

        return map.TryGetValue(derivedType, out typeName);
    }

    private Dictionary<string, Type> BuildMapByName()
    {
        var result = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        // Ensure that the last registration wins and becomes the default.
        foreach (var (derivedType, typeName) in derivedTypes)
        {
            result[typeName] = derivedType;
        }

        return result;
    }

    private Dictionary<Type, string> BuildMapByType()
    {
        var result = new Dictionary<Type, string>();

        // Ensure that the last registration wins and becomes the default.
        foreach (var (derivedType, typeName) in derivedTypes)
        {
            result[derivedType] = typeName;
        }

        return result;
    }
}
