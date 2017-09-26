// ==========================================================================
//  SimpleMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.Reflection
{
    public static class SimpleMapper
    {
        private sealed class ConversionPropertyMapper : PropertyMapper
        {
            private readonly Type targetType;

            public ConversionPropertyMapper(IPropertyAccessor srcAccessor, IPropertyAccessor dstAccessor, Type targetType)
                : base(srcAccessor, dstAccessor)
            {
                this.targetType = targetType;
            }

            public override void MapProperty(object source, object destination, CultureInfo culture)
            {
                var value = GetValue(source);

                if (value == null)
                {
                    return;
                }

                object converted;
                try
                {
                    converted = Convert.ChangeType(value, targetType, culture);

                    SetValue(destination, converted);
                }
                catch (InvalidCastException)
                {
                    if (targetType == typeof(string))
                    {
                        converted = value.ToString();

                        SetValue(destination, converted);
                    }
                }
            }
        }

        private class PropertyMapper
        {
            private readonly IPropertyAccessor srcAccessor;
            private readonly IPropertyAccessor dstAccessor;

            public PropertyMapper(IPropertyAccessor srcAccessor, IPropertyAccessor dstAccessor)
            {
                this.srcAccessor = srcAccessor;
                this.dstAccessor = dstAccessor;
            }

            public virtual void MapProperty(object source, object destination, CultureInfo culture)
            {
                var value = GetValue(source);

                SetValue(destination, value);
            }

            protected void SetValue(object destination, object value)
            {
                dstAccessor.Set(destination, value);
            }

            protected object GetValue(object source)
            {
                return srcAccessor.Get(source);
            }
        }

        private static class ClassMapper<TSource, TDestination>
            where TSource : class
            where TDestination : class
        {
            private static readonly PropertyMapper[] Mappers;

            private static readonly Type[] Convertibles =
            {
                typeof(bool),
                typeof(byte),
                typeof(char),
                typeof(decimal),
                typeof(float),
                typeof(double),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(string)
            };

            static ClassMapper()
            {
                var dstType = typeof(TDestination);
                var srcType = typeof(TSource);

                var destinationProperties = dstType.GetPublicProperties();

                var newMappers = new List<PropertyMapper>();

                foreach (var srcProperty in srcType.GetPublicProperties().Where(x => x.CanRead))
                {
                    var dstProperty = destinationProperties.FirstOrDefault(x => x.Name == srcProperty.Name);

                    if (dstProperty == null || !dstProperty.CanWrite)
                    {
                        continue;
                    }

                    var srcPropertyType = srcProperty.PropertyType;
                    var dstPropertyType = dstProperty.PropertyType;

                    if (srcPropertyType == dstPropertyType)
                    {
                        newMappers.Add(new PropertyMapper(new PropertyAccessor(srcType, srcProperty), new PropertyAccessor(dstType, dstProperty)));
                    }
                    else
                    {
                        if (Convertibles.Contains(dstPropertyType))
                        {
                            newMappers.Add(new ConversionPropertyMapper(new PropertyAccessor(srcType, srcProperty), new PropertyAccessor(dstType, dstProperty), dstPropertyType));
                        }
                    }
                }

                Mappers = newMappers.ToArray();
            }

            public static TDestination MapClass(TSource source, TDestination destination, CultureInfo culture)
            {
                foreach (var mapper in Mappers)
                {
                    mapper.MapProperty(source, destination, culture);
                }

                return destination;
            }
        }

        public static TDestination Map<TSource, TDestination>(TSource source)
            where TSource : class
            where TDestination : class, new()
        {
            return Map(source, new TDestination(), CultureInfo.CurrentCulture);
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
            where TSource : class
            where TDestination : class
        {
            return Map(source, destination, CultureInfo.CurrentCulture);
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, CultureInfo culture)
            where TSource : class
            where TDestination : class
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(culture, nameof(culture));
            Guard.NotNull(destination, nameof(destination));

            return ClassMapper<TSource, TDestination>.MapClass(source, destination, culture);
        }
    }
}
