// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using System.Runtime.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.System;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Web.Json;

public class JsonInheritanceConverter<T> : InheritanceConverterBase<T> where T : notnull
{
    private static readonly Lazy<Dictionary<string, Type>> DefaultMapping = new Lazy<Dictionary<string, Type>>(() =>
    {
        var baseName = typeof(T).Name;

        var result = new Dictionary<string, Type>();

        void AddType(Type type)
        {
            var typeName = type.Name;

            if (typeName.EndsWith(baseName, StringComparison.CurrentCulture))
            {
                typeName = typeName[..^baseName.Length];
            }

            result[typeName] = type;
        }

        foreach (var attribute in typeof(T).GetCustomAttributes<KnownTypeAttribute>())
        {
            if (attribute.Type != null)
            {
                if (!attribute.Type.IsAbstract)
                {
                    AddType(attribute.Type);
                }
            }
            else if (!string.IsNullOrWhiteSpace(attribute.MethodName))
            {
                var method = typeof(T).GetMethod(attribute.MethodName);

                if (method != null && method.IsStatic)
                {
                    var types = (IEnumerable<Type>)method.Invoke(null, Array.Empty<object>())!;

                    foreach (var type in types)
                    {
                        if (!type.IsAbstract)
                        {
                            AddType(type);
                        }
                    }
                }
            }
        }

        return result;
    });

    private readonly IReadOnlyDictionary<string, Type> mapping;

    public JsonInheritanceConverter()
        : this(null, DefaultMapping.Value)
    {
    }

    public JsonInheritanceConverter(string? discriminatorName)
        : this(discriminatorName, DefaultMapping.Value)
    {
    }

    public JsonInheritanceConverter(string? discriminatorName, IReadOnlyDictionary<string, Type> mapping)
        : base(GetDiscriminatorName(discriminatorName))
    {
        this.mapping = mapping ?? DefaultMapping.Value;
    }

    private static string GetDiscriminatorName(string? discriminatorName)
    {
        var attribute = typeof(T).GetCustomAttribute<JsonInheritanceConverterAttribute>();

        return attribute?.DiscriminatorName ?? discriminatorName ?? "discriminator";
    }

    public override Type GetDiscriminatorType(string name, Type typeToConvert)
    {
        if (!mapping.TryGetValue(name, out var type))
        {
            ThrowHelper.JsonException($"Could not find subtype of '{typeToConvert.Name}' with discriminator '{name}'.");
            return default!;
        }

        return type;
    }

    public override string GetDiscriminatorValue(Type type)
    {
        return mapping.FirstOrDefault(x => x.Value == type).Key ?? type.Name;
    }
}
