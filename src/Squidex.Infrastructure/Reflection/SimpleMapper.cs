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

            public ConversionPropertyMapper(
                IPropertyAccessor sourceAccessor,
                IPropertyAccessor targetAccessor,
                Type targetType)
                : base(sourceAccessor, targetAccessor)
            {
                this.targetType = targetType;
            }

            public override void MapProperty(object source, object target, CultureInfo culture)
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

                    SetValue(target, converted);
                }
                catch (InvalidCastException)
                {
                    if (targetType == typeof(string))
                    {
                        converted = value.ToString();

                        SetValue(target, converted);
                    }
                }
            }
        }

        private class PropertyMapper
        {
            private readonly IPropertyAccessor sourceAccessor;
            private readonly IPropertyAccessor targetAccessor;

            public PropertyMapper(IPropertyAccessor sourceAccessor, IPropertyAccessor targetAccessor)
            {
                this.sourceAccessor = sourceAccessor;
                this.targetAccessor = targetAccessor;
            }

            public virtual void MapProperty(object source, object target, CultureInfo culture)
            {
                var value = GetValue(source);

                SetValue(target, value);
            }

            protected void SetValue(object destination, object value)
            {
                targetAccessor.Set(destination, value);
            }

            protected object GetValue(object source)
            {
                return sourceAccessor.Get(source);
            }
        }

        private static class ClassMapper<TSource, TTarget> where TSource : class where TTarget : class
        {
            private static readonly List<PropertyMapper> Mappers = new List<PropertyMapper>();

            static ClassMapper()
            {
                var sourceClassType = typeof(TSource);
                var sourceProperties =
                    sourceClassType.GetPublicProperties()
                        .Where(x => x.CanRead).ToList();

                var targetClassType = typeof(TTarget);
                var targetProperties =
                    targetClassType.GetPublicProperties()
                        .Where(x => x.CanWrite).ToList();

                foreach (var sourceProperty in sourceProperties)
                {
                    var targetProperty = targetProperties.FirstOrDefault(x => x.Name == sourceProperty.Name);

                    if (targetProperty == null)
                    {
                        continue;
                    }

                    var sourceType = sourceProperty.PropertyType;
                    var targetType = targetProperty.PropertyType;

                    if (sourceType == targetType)
                    {
                        Mappers.Add(new PropertyMapper(
                            new PropertyAccessor(sourceClassType, sourceProperty),
                            new PropertyAccessor(targetClassType, targetProperty)));
                    }
                    else if (targetType.Implements<IConvertible>())
                    {
                        Mappers.Add(new ConversionPropertyMapper(
                            new PropertyAccessor(sourceClassType, sourceProperty),
                            new PropertyAccessor(targetClassType, targetProperty),
                            targetType));
                    }
                }
            }

            public static TTarget MapClass(TSource source, TTarget destination, CultureInfo culture)
            {
                foreach (var mapper in Mappers)
                {
                    mapper.MapProperty(source, destination, culture);
                }

                return destination;
            }
        }

        public static TTarget Map<TSource, TTarget>(TSource source)
            where TSource : class
            where TTarget : class, new()
        {
            return Map(source, new TTarget(), CultureInfo.CurrentCulture);
        }

        public static TTarget Map<TSource, TTarget>(TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            return Map(source, target, CultureInfo.CurrentCulture);
        }

        public static TTarget Map<TSource, TTarget>(TSource source, TTarget target, CultureInfo culture)
            where TSource : class
            where TTarget : class
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(culture, nameof(culture));
            Guard.NotNull(target, nameof(target));

            return ClassMapper<TSource, TTarget>.MapClass(source, target, culture);
        }
    }
}
