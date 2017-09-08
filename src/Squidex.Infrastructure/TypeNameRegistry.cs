// ==========================================================================
//  TypeNameRegistry.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Squidex.Infrastructure
{
    public sealed class TypeNameRegistry
    {
        private readonly Dictionary<Type, string> namesByType = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> typesByName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public TypeNameRegistry Map(Type type)
        {
            Guard.NotNull(type, nameof(type));

            var typeNameAttribute = type.GetTypeInfo().GetCustomAttribute<TypeNameAttribute>();

            if (typeNameAttribute != null)
            {
                Map(type, typeNameAttribute.TypeName);
            }

            return this;
        }

        public TypeNameRegistry MapObsolete(Type type, string name)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(name, nameof(name));

            lock (namesByType)
            {
                try
                {
                    typesByName.Add(name, type);
                }
                catch (ArgumentException)
                {
                    if (typesByName[name] != type)
                    {
                        var message = $"The name '{name}' is already registered with type '{typesByName[name]}'";

                        throw new ArgumentException(message, nameof(type));
                    }
                }
            }

            return this;
        }

        public TypeNameRegistry Map(Type type, string name)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(name, nameof(name));

            lock (namesByType)
            {
                try
                {
                    namesByType.Add(type, name);
                }
                catch (ArgumentException)
                {
                    if (namesByType[type] != name)
                    {
                        var message = $"The type '{type}' is already registered with name '{namesByType[type]}'";

                        throw new ArgumentException(message, nameof(type));
                    }
                }

                try
                {
                    typesByName.Add(name, type);
                }
                catch (ArgumentException)
                {
                    if (typesByName[name] != type)
                    {
                        var message = $"The name '{name}' is already registered with type '{typesByName[name]}'";

                        throw new ArgumentException(message, nameof(type));
                    }
                }
            }

            return this;
        }

        public TypeNameRegistry Map(Assembly assembly)
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

        public string GetName<T>()
        {
            return GetName(typeof(T));
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
