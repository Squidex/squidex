// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using NJsonSchema.Converters;
using Squidex.Infrastructure;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Web.Json
{
    public class TypedJsonInheritanceConverter<T> : JsonInheritanceConverter
    {
        private static readonly Lazy<Dictionary<string, Type>> DefaultMapping = new Lazy<Dictionary<string, Type>>(() =>
        {
            var baseName = typeof(T).Name;

            var result = new Dictionary<string, Type>();

            void AddType(Type type)
            {
                var discriminator = type.Name;

                if (discriminator.EndsWith(baseName, StringComparison.CurrentCulture))
                {
                    discriminator = discriminator.Substring(0, discriminator.Length - baseName.Length);
                }

                result[discriminator] = type;
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

        public TypedJsonInheritanceConverter(string discriminator)
            : this(discriminator, DefaultMapping.Value)
        {
        }

        public TypedJsonInheritanceConverter(string discriminator, IReadOnlyDictionary<string, Type> mapping)
            : base(typeof(T), discriminator)
        {
            this.mapping = mapping ?? DefaultMapping.Value;
        }

        protected override Type GetDiscriminatorType(JObject jObject, Type objectType, string discriminatorValue)
        {
            return mapping.GetOrDefault(discriminatorValue) ?? throw new InvalidOperationException($"Could not find subtype of '{objectType.Name}' with discriminator '{discriminatorValue}'.");
        }

        public override string GetDiscriminatorValue(Type type)
        {
            return mapping.FirstOrDefault(x => x.Value == type).Key ?? type.Name;
        }
    }
}