// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Google.Protobuf.Reflection;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.Reflection;

public sealed class TypeRegistry
{
    private readonly Dictionary<Type, TypeConfig> configs = new Dictionary<Type, TypeConfig>();

    public TypeConfig this[Type type]
    {
        get => configs.GetOrAddNew(type);
    }

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
        this[typeof(T)].Add(derivedType, typeName);

        return this;
    }

    public TypeRegistry Discriminator<T>(string discriminiatorProperty)
    {
        this[typeof(T)].DiscriminatorProperty = discriminiatorProperty;

        return this;
    }

    public string GetName<TBase, TDerived>() where TDerived : TBase where TBase : class
    {
        return GetName<TBase>(typeof(TDerived));
    }

    public string GetName<T>(Type derivedType) where T : class
    {
        if (!this[typeof(IEvent)].TryGetName(derivedType, out var name))
        {
            ThrowHelper.InvalidOperationException($"Unknown derived type {derivedType}.");
            return default!;
        }

        return name;
    }
}
