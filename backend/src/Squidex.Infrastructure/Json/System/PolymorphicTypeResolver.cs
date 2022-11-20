// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Json.System;

public sealed class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    private readonly TypeRegistry typeRegistry;

    public PolymorphicTypeResolver(TypeRegistry typeRegistry)
    {
        Guard.NotNull(typeRegistry);

        this.typeRegistry = typeRegistry;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);

        var baseType = type.BaseType;

        while (baseType != null)
        {
            if (typeRegistry.TryGetConfig(baseType, out var config) && config.TryGetName(type, out var typeName))
            {
                var discriminiatorName = config.DiscriminatorProperty ?? Constants.DefaultDiscriminatorProperty;
                var discriminatorField = typeInfo.CreateJsonPropertyInfo(typeof(string), discriminiatorName);

                discriminatorField.Get = x =>
                {
                    return typeName;
                };

                typeInfo.Properties.Insert(0, discriminatorField);
            }

            baseType = baseType.BaseType;
        }

        return typeInfo;
    }
}
