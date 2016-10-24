// ==========================================================================
//  TypeNameRegistry.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace PinkParrot.Infrastructure
{
    public class TypeNameRegistry
    {
        private static readonly Dictionary<Type, string> namesByType = new Dictionary<Type, string>();
        private static readonly Dictionary<string, Type> typesByName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public static void Map(Type type, string name)
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
                    var message = $"The type '{type}' is already registered with name '{namesByType[type]}'";

                    throw new ArgumentException(message, nameof(type));
                }

                try
                {
                    typesByName.Add(name, type);
                }
                catch (ArgumentException)
                {
                    var message = $"The name '{name}' is already registered with type '{typesByName[name]}'";

                    throw new ArgumentException(message, nameof(type));
                }
            }
        }

        public static void Map(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var typeNameAttribute = type.GetTypeInfo().GetCustomAttribute<TypeNameAttribute>();

                if (!string.IsNullOrWhiteSpace(typeNameAttribute?.TypeName))
                {
                    Map(type, typeNameAttribute.TypeName);
                }
            }
        }

        public static string GetName<T>()
        {
            return GetName(typeof(T));
        }

        public static string GetName(Type type)
        {
            var result = namesByType.GetOrDefault(type);

            if (result == null)
            {
                throw new ArgumentException($"There is no name for type '{type}");
            }

            return result;
        }

        public static Type GetType(string name)
        {
            var result = typesByName.GetOrDefault(name);

            if (result == null)
            {
                throw new ArgumentException($"There is no type for name '{name}");
            }

            return result;
        }
    }
}
