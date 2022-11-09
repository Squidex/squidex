// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Json.System;

public sealed class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    private readonly Dictionary<Type, Config> configs = new Dictionary<Type, Config>();
    private readonly TypeNameRegistry typeNameRegistry;

    private sealed class Config
    {
        public string DiscriminatorProperty { get; set; }

        public Dictionary<string, Type> Mappings { get; } = new Dictionary<string, Type>();
    }

    public PolymorphicTypeResolver(TypeNameRegistry typeNameRegistry)
    {
        Guard.NotNull(typeNameRegistry);

        this.typeNameRegistry = typeNameRegistry;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);

        if (configs.TryGetValue(type, out var config))
        {
            var polymorphicOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = config.DiscriminatorProperty
            };

            foreach (var (discriminiator, derivedType) in config.Mappings)
            {
                polymorphicOptions.DerivedTypes.Add(new JsonDerivedType(derivedType, discriminiator));
            }

            typeInfo.PolymorphismOptions = polymorphicOptions;
        }
        else
        {
            var baseType = type.BaseType;

            if (baseType == null || baseType == typeof(object))
            {
                return typeInfo;
            }

            var baseInfo = GetTypeInfo(baseType, options);

            if (baseInfo.Converter is IInheritanceConverter inheritanceConverter)
            {
                var discriminatorField = typeInfo.CreateJsonPropertyInfo(typeof(string), inheritanceConverter.DiscriminatorName);
                var discriminatorValue = inheritanceConverter.GetDiscriminatorValue(type);

                discriminatorField.Get = x =>
                {
                    return discriminatorValue;
                };

                typeInfo.Properties.Insert(0, discriminatorField);
            }
        }

        return typeInfo;
    }

    public PolymorphicTypeResolver Add<T>(string discriminatorProperty = "$type")
    {
        return Add<T>(discriminatorProperty, typeof(T).Assembly);
    }

    public PolymorphicTypeResolver Add<T>(string discriminatorProperty, params Assembly[] assemblies)
    {
        Guard.NotNullOrEmpty(discriminatorProperty);
        Guard.NotNull(assemblies);

        var mapping = new Config
        {
            DiscriminatorProperty = discriminatorProperty
        };

        var baseType = typeof(T);

        foreach (var assembly in assemblies)
        {
            foreach (var derivedType in assembly.GetTypes().Where(x => x.BaseType == baseType))
            {
                var typeName = typeNameRegistry.GetNameOrNull(derivedType);

                if (typeName != null)
                {
                    mapping.Mappings[typeName] = derivedType;
                }
            }
        }

        configs[baseType] = mapping;

        return this;
    }

    public PolymorphicTypeResolver Add<T>(string discriminatorProperty, Dictionary<string, Type> map)
    {
        Guard.NotNullOrEmpty(discriminatorProperty);
        Guard.NotNull(map);

        var config = new Config
        {
            DiscriminatorProperty = discriminatorProperty
        };

        config.Mappings.AddRange(map);

        configs[typeof(T)] = config;

        return this;
    }
}
