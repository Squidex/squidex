// ==========================================================================
//  TypeNameSerializationBinder.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace PinkParrot.Infrastructure.Json
{
    public class TypeNameSerializationBinder : DefaultSerializationBinder
    {
        private readonly Dictionary<Type, string> namesByType = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> typesByName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public TypeNameSerializationBinder Map(Type type, string name)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(name, nameof(name));

            namesByType.Add(type, name);
            typesByName.Add(name, type);

            return this;
        }

        public TypeNameSerializationBinder Map(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var typeNameAttribute = type.GetTypeInfo().GetCustomAttribute<TypeNameAttribute>();

                if (!string.IsNullOrWhiteSpace(typeNameAttribute?.TypeName))
                {
                    Map(type, typeNameAttribute.TypeName);
                }
            }

            return this;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            var type = typesByName.GetOrDefault(typeName);

            return type ?? base.BindToType(assemblyName, typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;

            var name = namesByType.GetOrDefault(serializedType);

            if (name != null)
            {
                typeName = name;
            }
            else
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }
    }
}

