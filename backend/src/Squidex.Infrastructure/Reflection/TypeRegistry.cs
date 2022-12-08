// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.Reflection;

public sealed class TypeRegistry
{
    private readonly Dictionary<Type, TypeConfig> configs = new Dictionary<Type, TypeConfig>();

    public TypeRegistry(IEnumerable<ITypeProvider>? providers = null)
    {
        if (providers != null)
        {
            foreach (var provider in providers)
            {
                Map(provider);
            }
        }
    }

    public TypeRegistry Map(ITypeProvider provider)
    {
        Guard.NotNull(provider);

        provider.Map(this);

        return this;
    }

    public TypeRegistry Add<TBase, TDerived>(string typeName) where TDerived : TBase where TBase : class
    {
        return Add<TBase>(typeof(TDerived), typeName);
    }

    public TypeRegistry Add<T>(Type derivedType, string typeName) where T : class
    {
        lock (configs)
        {
            configs.GetOrAddNew(typeof(T)).Add(derivedType, typeName);
        }

        return this;
    }

    public TypeRegistry Discriminator<T>(string? discriminiatorProperty)
    {
        lock (configs)
        {
            configs.GetOrAddNew(typeof(T)).DiscriminatorProperty ??= discriminiatorProperty;
        }

        return this;
    }

    public string GetName<TBase, TDerived>() where TDerived : TBase where TBase : class
    {
        return GetName<TBase>(typeof(TDerived));
    }

    public string GetName<T>(Type derivedType) where T : class
    {
        if (!TryGetName<T>(derivedType, out var name))
        {
            ThrowHelper.ArgumentException($"Unknown derived type {derivedType}.", nameof(derivedType));
            return default!;
        }

        return name;
    }

    public Type GetType<T>(string typeName) where T : class
    {
        if (!TryGetType<T>(typeName, out var name))
        {
            ThrowHelper.ArgumentException($"Unknown derived type {typeName}.", nameof(typeName));
            return default!;
        }

        return name;
    }

    public bool TryGetName<T>(Type derivedType, [MaybeNullWhen(false)] out string typeName) where T : class
    {
        typeName = null!;

        return TryGetConfig<T>(out var config) && config.TryGetName(derivedType, out typeName);
    }

    public bool TryGetType<T>(string typeName, [MaybeNullWhen(false)] out Type derivedType) where T : class
    {
        derivedType = null!;

        return TryGetConfig<T>(out var config) && config.TryGetType(typeName, out derivedType);
    }

    public bool TryGetConfig<T>([MaybeNullWhen(false)] out TypeConfig config) where T : class
    {
        return TryGetConfig(typeof(T), out config);
    }

    public bool TryGetConfig(Type baseType, [MaybeNullWhen(false)] out TypeConfig config)
    {
        return configs.TryGetValue(baseType, out config);
    }
}
