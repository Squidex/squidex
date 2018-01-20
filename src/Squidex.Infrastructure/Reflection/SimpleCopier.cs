// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.Reflection
{
    public static class SimpleCopier
    {
        private struct PropertyMapper
        {
            private readonly IPropertyAccessor accessor;
            private readonly Func<object, object> converter;

            public PropertyMapper(IPropertyAccessor accessor, Func<object, object> converter)
            {
                this.accessor = accessor;
                this.converter = converter;
            }

            public void MapProperty(object source, object target)
            {
                var value = converter(accessor.Get(source));

                accessor.Set(target, value);
            }
        }

        private static class ClassCopier<T> where T : class, new()
        {
            private static readonly List<PropertyMapper> Mappers = new List<PropertyMapper>();

            static ClassCopier()
            {
                var type = typeof(T);

                foreach (var property in type.GetPublicProperties())
                {
                    if (!property.CanWrite || !property.CanRead)
                    {
                        continue;
                    }

                    var accessor = new PropertyAccessor(type, property);

                    if (property.PropertyType.Implements<ICloneable>())
                    {
                        Mappers.Add(new PropertyMapper(accessor, x => ((ICloneable)x)?.Clone()));
                    }
                    else
                    {
                        Mappers.Add(new PropertyMapper(accessor, x => x));
                    }
                }
            }

            public static T CopyThis(T source)
            {
                var destination = new T();

                foreach (var mapper in Mappers)
                {
                    mapper.MapProperty(source, destination);
                }

                return destination;
            }
        }

        public static T Copy<T>(this T source) where T : class, new()
        {
            Guard.NotNull(source, nameof(source));

            return ClassCopier<T>.CopyThis(source);
        }
    }
}
