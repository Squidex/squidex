// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using System.Globalization;
using Squidex.Infrastructure.Reflection.Internal;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.Reflection;

public static class SimpleMapper
{
    private struct MappingContext
    {
        required public CultureInfo Culture { get; init; }

        required public bool NullableAsOptional { get; init; }
    }

    private sealed class StringConversionPropertyMapper : PropertyMapper
    {
        public StringConversionPropertyMapper(
            PropertyAccessor sourceAccessor,
            PropertyAccessor targetAccessor)
            : base(sourceAccessor, targetAccessor)
        {
        }

        public override void MapProperty(object source, object target, ref MappingContext context)
        {
            var value = GetValue(source);

            SetValue(target, value?.ToString());
        }
    }

    private sealed class NullablePropertyMapper : PropertyMapper
    {
        private readonly object? defaultValue;

        public NullablePropertyMapper(
            PropertyAccessor sourceAccessor,
            PropertyAccessor targetAccessor,
            object? defaultValue)
            : base(sourceAccessor, targetAccessor)
        {
            this.defaultValue = defaultValue;
        }

        public override void MapProperty(object source, object target, ref MappingContext context)
        {
            var value = GetValue(source);

            if (value == null)
            {
                if (context.NullableAsOptional)
                {
                    return;
                }
                else
                {
                    value = defaultValue;
                }
            }

            SetValue(target, value);
        }
    }

    private sealed class ConversionPropertyMapper : PropertyMapper
    {
        private readonly Type targetType;

        public ConversionPropertyMapper(
            PropertyAccessor sourceAccessor,
            PropertyAccessor targetAccessor,
            Type targetType)
            : base(sourceAccessor, targetAccessor)
        {
            this.targetType = targetType;
        }

        public override void MapProperty(object source, object target, ref MappingContext context)
        {
            var value = GetValue(source);

            if (value == null)
            {
                return;
            }

            try
            {
                var converted = Convert.ChangeType(value, targetType, context.Culture);

                SetValue(target, converted);
            }
            catch
            {
                return;
            }
        }
    }

    private sealed class TypeConverterPropertyMapper : PropertyMapper
    {
        private readonly TypeConverter converter;

        public TypeConverterPropertyMapper(
            PropertyAccessor sourceAccessor,
            PropertyAccessor targetAccessor,
            TypeConverter converter)
            : base(sourceAccessor, targetAccessor)
        {
            this.converter = converter;
        }

        public override void MapProperty(object source, object target, ref MappingContext context)
        {
            var value = GetValue(source);

            if (value == null)
            {
                return;
            }

            try
            {
                var converted = converter.ConvertFrom(null, context.Culture, value);

                SetValue(target, converted);
            }
            catch
            {
                return;
            }
        }
    }

    private class PropertyMapper
    {
        private readonly PropertyAccessor sourceAccessor;
        private readonly PropertyAccessor targetAccessor;

        public PropertyMapper(PropertyAccessor sourceAccessor, PropertyAccessor targetAccessor)
        {
            this.sourceAccessor = sourceAccessor;
            this.targetAccessor = targetAccessor;
        }

        public virtual void MapProperty(object source, object target, ref MappingContext context)
        {
            var value = GetValue(source);

            SetValue(target, value);
        }

        protected void SetValue(object destination, object? value)
        {
            targetAccessor.Set(destination, value);
        }

        protected object? GetValue(object source)
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
                var targetProperty = targetProperties.Find(x => x.Name == sourceProperty.Name);

                if (targetProperty == null)
                {
                    continue;
                }

                var sourceType = sourceProperty.PropertyType;
                var targetType = targetProperty.PropertyType;

                if (sourceType == targetType)
                {
                    Mappers.Add(new PropertyMapper(
                        new PropertyAccessor(sourceProperty),
                        new PropertyAccessor(targetProperty)));
                }
                else if (targetType == typeof(string))
                {
                    Mappers.Add(new StringConversionPropertyMapper(
                        new PropertyAccessor(sourceProperty),
                        new PropertyAccessor(targetProperty)));
                }
                else if (IsNullableOf(sourceType, targetType))
                {
                    Mappers.Add(new NullablePropertyMapper(
                        new PropertyAccessor(sourceProperty),
                        new PropertyAccessor(targetProperty),
                        Activator.CreateInstance(targetType)));
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(targetType);

                    if (converter.CanConvertFrom(sourceType))
                    {
                        Mappers.Add(new TypeConverterPropertyMapper(
                            new PropertyAccessor(sourceProperty),
                            new PropertyAccessor(targetProperty),
                            converter));
                    }
                    else if (sourceType.Implements<IConvertible>() || targetType.Implements<IConvertible>())
                    {
                        Mappers.Add(new ConversionPropertyMapper(
                            new PropertyAccessor(sourceProperty),
                            new PropertyAccessor(targetProperty),
                            targetType));
                    }
                }
            }

            static bool IsNullableOf(Type type, Type wrappedType)
            {
                return type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                    type.GenericTypeArguments[0] == wrappedType;
            }
        }

        public static TTarget MapClass(TSource source, TTarget destination, ref MappingContext context)
        {
            for (var i = 0; i < Mappers.Count; i++)
            {
                var mapper = Mappers[i];

                mapper.MapProperty(source, destination, ref context);
            }

            return destination;
        }
    }

    public static TTarget Map<TSource, TTarget>(TSource source, TTarget target)
        where TSource : class
        where TTarget : class
    {
        return Map(source, target, CultureInfo.CurrentCulture, true);
    }

    public static TTarget Map<TSource, TTarget>(TSource source, TTarget target, CultureInfo culture, bool nullableAsOptional)
        where TSource : class
        where TTarget : class
    {
        Guard.NotNull(source);
        Guard.NotNull(culture);
        Guard.NotNull(target);

        var context = new MappingContext
        {
            Culture = culture,
            NullableAsOptional = nullableAsOptional
        };

        return ClassMapper<TSource, TTarget>.MapClass(source, target, ref context);
    }
}
