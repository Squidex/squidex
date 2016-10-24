// ==========================================================================
//  ReflectionExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Squidex.Infrastructure.Reflection
{
    public static class ReflectionExtensions
    {
        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            const BindingFlags bindingFlags =
                BindingFlags.FlattenHierarchy |
                BindingFlags.Public |
                BindingFlags.Instance;

            if (!type.GetTypeInfo().IsInterface)
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
    }
}
