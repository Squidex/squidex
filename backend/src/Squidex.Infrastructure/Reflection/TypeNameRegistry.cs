// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Squidex.Infrastructure.Reflection
{
    public sealed class TypeNameRegistry
    {
        private readonly Dictionary<Type, string> namesByType = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> typesByName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public TypeNameRegistry(IEnumerable<ITypeProvider>? providers = null)
        {
            if (providers != null)
            {
                foreach (var provider in providers)
                {
                    Map(provider);
                }
            }
        }

        public TypeNameRegistry MapObsolete(Type type, string name)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(name, nameof(name));

            lock (namesByType)
            {
                if (typesByName.TryGetValue(name, out var existingType) && existingType != type)
                {
                    var message = $"The name '{name}' is already registered with type '{typesByName[name]}'";

                    throw new ArgumentException(message, nameof(type));
                }

                typesByName[name] = type;
            }

            return this;
        }

        public TypeNameRegistry Map(ITypeProvider provider)
        {
            Guard.NotNull(provider, nameof(provider));

            provider.Map(this);

            return this;
        }

        public TypeNameRegistry Map(Type type)
        {
            Guard.NotNull(type, nameof(type));

            var typeNameAttribute = type.GetCustomAttribute<TypeNameAttribute>();

            if (!string.IsNullOrWhiteSpace(typeNameAttribute?.TypeName))
            {
                Map(type, typeNameAttribute.TypeName);
            }

            return this;
        }

        public TypeNameRegistry Map(Type type, string name)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(name, nameof(name));

            lock (namesByType)
            {
                if (namesByType.TryGetValue(type, out var existingName) && existingName != name)
                {
                    var message = $"The type '{type}' is already registered with name '{namesByType[type]}'";

                    throw new ArgumentException(message, nameof(type));
                }

                namesByType[type] = name;

                if (typesByName.TryGetValue(name, out var existingType) && existingType != type)
                {
                    var message = $"The name '{name}' is already registered with type '{typesByName[name]}'";

                    throw new ArgumentException(message, nameof(type));
                }

                typesByName[name] = type;
            }

            return this;
        }

        public TypeNameRegistry MapUnmapped(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!namesByType.ContainsKey(type))
                {
                    Map(type);
                }
            }

            return this;
        }

        public string GetName<T>()
        {
            return GetName(typeof(T));
        }

        public string GetNameOrNull<T>()
        {
            return GetNameOrNull(typeof(T));
        }

        public string GetNameOrNull(Type type)
        {
            var result = namesByType.GetOrDefault(type);

            return result;
        }

        public Type GetTypeOrNull(string name)
        {
            var result = typesByName.GetOrDefault(name);

            return result;
        }

        public string GetName(Type type)
        {
            var result = namesByType.GetOrDefault(type);

            if (result == null)
            {
                throw new TypeNameNotFoundException($"There is no name for type '{type}");
            }

            return result;
        }

        public Type GetType(string name)
        {
            var result = typesByName.GetOrDefault(name);

            if (result == null)
            {
                throw new TypeNameNotFoundException($"There is no type for name '{name}");
            }

            return result;
        }
    }
}
