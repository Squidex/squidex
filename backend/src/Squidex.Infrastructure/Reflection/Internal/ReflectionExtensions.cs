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

namespace Squidex.Infrastructure.Reflection.Internal
{
    public static class ReflectionExtensions
    {
        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            const BindingFlags bindingFlags =
                BindingFlags.FlattenHierarchy |
                BindingFlags.Public |
                BindingFlags.Instance;

            if (!type.IsInterface)
            {
                return type.GetProperties(bindingFlags);
            }

            var flattenProperties = new HashSet<PropertyInfo>();

            var considered = new List<Type>
            {
                type
            };

            var queue = new Queue<Type>();

            queue.Enqueue(type);

            while (queue.Count > 0)
            {
                var subType = queue.Dequeue();

                foreach (var subInterface in subType.GetInterfaces())
                {
                    if (considered.Contains(subInterface))
                    {
                        continue;
                    }

                    considered.Add(subInterface);

                    queue.Enqueue(subInterface);
                }

                var typeProperties = subType.GetProperties(bindingFlags);

                foreach (var property in typeProperties)
                {
                    flattenProperties.Add(property);
                }
            }

            return flattenProperties.ToArray();
        }

        public static bool Implements<T>(this Type type)
        {
            return type.GetInterfaces().Contains(typeof(T));
        }
    }
}
