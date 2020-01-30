// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Reflection;
using Squidex.Infrastructure.Reflection.Internal;

namespace Squidex.Infrastructure.Reflection.Equality
{
    public sealed class ObjectComparer : IDeepComparer
    {
        private readonly PropertyAccessor[] propertyAccessors;
        private readonly IDeepComparer valueComparer;

        public ObjectComparer(IDeepComparer valueComparer, Type type)
        {
            propertyAccessors =
                type.GetPublicProperties()
                    .Where(x => x.CanRead)
                    .Where(x => x.GetCustomAttribute<IgnoreEqualsAttribute>() == null)
                    .Select(x => new PropertyAccessor(x.DeclaringType!, x))
                    .ToArray();

            this.valueComparer = valueComparer;
        }

        public bool IsEquals(object? x, object? y)
        {
            for (var i = 0; i < propertyAccessors.Length; i++)
            {
                var property = propertyAccessors[i];

                var lhs = property.Get(x!);
                var rhs = property.Get(y!);

                if (!valueComparer.IsEquals(lhs, rhs))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
