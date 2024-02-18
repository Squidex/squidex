// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Squidex.Infrastructure.Reflection.Internal;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.Reflection;

public static class SimpleMapper
{
    internal readonly record struct MappingContext
    {
        required public CultureInfo Culture { get; init; }

        required public bool NullableAsOptional { get; init; }
    }

    internal interface IPropertyMapper<TSource, TTarget>
    {
        void MapProperty(TSource source, TTarget target, ref MappingContext context);
    }

    private static class SimplePropertyMapper
    {
        private static readonly MethodInfo CreateMethod =
            typeof(SimplePropertyMapper)
                    .GetMethod(nameof(CreateCore),
                        BindingFlags.Static |
                        BindingFlags.NonPublic)!;

        public static IPropertyMapper<TSource, TTarget> Create<TSource, TTarget>(Type valueType, PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            var method = CreateMethod.MakeGenericMethod(typeof(TSource), typeof(TTarget), valueType);

            return (IPropertyMapper<TSource, TTarget>)method.Invoke(null, [sourceProperty, targetProperty])!;
        }

        private static IPropertyMapper<TSource, TTarget> CreateCore<TSource, TTarget, TValue>(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            return new SimplePropertyMapper<TSource, TTarget, TValue>(sourceProperty, targetProperty);
        }
    }

    private sealed class SimplePropertyMapper<TSource, TTarget, TValue> : IPropertyMapper<TSource, TTarget>
    {
        private readonly PropertyAccessor.Getter<TSource, TValue> getter;
        private readonly PropertyAccessor.Setter<TTarget, TValue> setter;

        public SimplePropertyMapper(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            getter = PropertyAccessor.CreateGetter<TSource, TValue>(sourceProperty);
            setter = PropertyAccessor.CreateSetter<TTarget, TValue>(targetProperty);
        }

        public void MapProperty(TSource source, TTarget target, ref MappingContext context)
        {
            var value = getter(source);

            setter(target, value);
        }
    }

    private static class StringConversionPropertyMapper
    {
        private static readonly MethodInfo CreateMethod =
            typeof(StringConversionPropertyMapper)
                    .GetMethod(nameof(CreateCore),
                        BindingFlags.Static |
                        BindingFlags.NonPublic)!;

        public static IPropertyMapper<TSource, TTarget> Create<TSource, TTarget>(Type valueType, PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            var method = CreateMethod.MakeGenericMethod(typeof(TSource), typeof(TTarget), valueType);

            return (IPropertyMapper<TSource, TTarget>)method.Invoke(null, [sourceProperty, targetProperty])!;
        }

        private static IPropertyMapper<TSource, TTarget> CreateCore<TSource, TTarget, TValue>(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            return new StringConversionPropertyMapper<TSource, TTarget, TValue>(sourceProperty, targetProperty);
        }
    }

    private sealed class StringConversionPropertyMapper<TSource, TTarget, TValue> : IPropertyMapper<TSource, TTarget>
    {
        private readonly PropertyAccessor.Getter<TSource, TValue> getter;
        private readonly PropertyAccessor.Setter<TTarget, string> setter;

        public StringConversionPropertyMapper(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            getter = PropertyAccessor.CreateGetter<TSource, TValue>(sourceProperty);
            setter = PropertyAccessor.CreateSetter<TTarget, string>(targetProperty);
        }

        public void MapProperty(TSource source, TTarget target, ref MappingContext context)
        {
            var value = getter(source);

            setter(target, value?.ToString()!);
        }
    }

    private static class NullablePropertyMapper
    {
        private static readonly MethodInfo CreateMethod =
            typeof(NullablePropertyMapper)
                    .GetMethod(nameof(CreateCore),
                        BindingFlags.Static |
                        BindingFlags.NonPublic)!;

        public static IPropertyMapper<TSource, TTarget> Create<TSource, TTarget>(Type valueType, PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            var method = CreateMethod.MakeGenericMethod(typeof(TSource), typeof(TTarget), valueType);

            return (IPropertyMapper<TSource, TTarget>)method.Invoke(null, [sourceProperty, targetProperty])!;
        }

        private static IPropertyMapper<TSource, TTarget> CreateCore<TSource, TTarget, TValue>(PropertyInfo sourceProperty, PropertyInfo targetProperty) where TValue : struct
        {
            return new NullablePropertyMapper<TSource, TTarget, TValue>(sourceProperty, targetProperty);
        }
    }

    private sealed class NullablePropertyMapper<TSource, TTarget, TValue> : IPropertyMapper<TSource, TTarget> where TValue : struct
    {
        private readonly PropertyAccessor.Getter<TSource, TValue?> getter;
        private readonly PropertyAccessor.Setter<TTarget, TValue> setter;

        public NullablePropertyMapper(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            getter = PropertyAccessor.CreateGetter<TSource, TValue?>(sourceProperty);
            setter = PropertyAccessor.CreateSetter<TTarget, TValue>(targetProperty);
        }

        public void MapProperty(TSource source, TTarget target, ref MappingContext context)
        {
            var value = getter(source);

            if (value is null)
            {
                if (context.NullableAsOptional)
                {
                    return;
                }
                else
                {
                    value = default(TValue);
                }
            }

            setter(target, value.Value);
        }
    }

    private static class ConversionPropertyMapper
    {
        private static readonly MethodInfo CreateMethod =
            typeof(ConversionPropertyMapper)
                    .GetMethod(nameof(CreateCore),
                        BindingFlags.Static |
                        BindingFlags.NonPublic)!;

        public static IPropertyMapper<TSource, TTarget> Create<TSource, TTarget>(Type sourceType, PropertyInfo sourceProperty, Type targetType, PropertyInfo targetProperty)
        {
            var method = CreateMethod.MakeGenericMethod(typeof(TSource), typeof(TTarget), sourceType, targetType);

            return (IPropertyMapper<TSource, TTarget>)method.Invoke(null, [sourceProperty, targetProperty])!;
        }

        private static IPropertyMapper<TSource, TTarget> CreateCore<TSource, TTarget, TSourceValue, TTargetValue>(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            return new ConversionPropertyMapper<TSource, TTarget, TSourceValue, TTargetValue>(sourceProperty, targetProperty);
        }
    }

    private sealed class ConversionPropertyMapper<TSource, TTarget, TSourceValue, TTargetValue> : IPropertyMapper<TSource, TTarget>
    {
        private readonly PropertyAccessor.Getter<TSource, TSourceValue> getter;
        private readonly PropertyAccessor.Setter<TTarget, TTargetValue> setter;

        public ConversionPropertyMapper(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            getter = PropertyAccessor.CreateGetter<TSource, TSourceValue>(sourceProperty);
            setter = PropertyAccessor.CreateSetter<TTarget, TTargetValue>(targetProperty);
        }

        public void MapProperty(TSource source, TTarget target, ref MappingContext context)
        {
            var value = (object?)getter(source);

            if (value == null)
            {
                return;
            }

            try
            {
                var converted = (TTargetValue)Convert.ChangeType(value, typeof(TTargetValue), context.Culture);

                setter(target, converted);
            }
            catch
            {
                return;
            }
        }
    }

    private static class TypeConverterPropertyMapper
    {
        private static readonly MethodInfo CreateMethod =
            typeof(TypeConverterPropertyMapper)
                .GetMethod(nameof(CreateCore),
                    BindingFlags.Static |
                    BindingFlags.NonPublic)!;

        public static IPropertyMapper<TSource, TTarget> Create<TSource, TTarget>(Type sourceType, PropertyInfo sourceProperty, Type targetType, PropertyInfo targetProperty, TypeConverter typeConverter)
        {
            var method = CreateMethod.MakeGenericMethod(typeof(TSource), typeof(TTarget), sourceType, targetType);

            return (IPropertyMapper<TSource, TTarget>)method.Invoke(null, [sourceProperty, targetProperty, typeConverter])!;
        }

        private static IPropertyMapper<TSource, TTarget> CreateCore<TSource, TTarget, TSourceValue, TTargetValue>(PropertyInfo sourceProperty, PropertyInfo targetProperty, TypeConverter typeConverter)
        {
            return new TypeConverterPropertyMapper<TSource, TTarget, TSourceValue, TTargetValue>(sourceProperty, targetProperty, typeConverter);
        }
    }

    private sealed class TypeConverterPropertyMapper<TSource, TTarget, TSourceType, TTargetType> : IPropertyMapper<TSource, TTarget>
    {
        private readonly PropertyAccessor.Getter<TSource, TSourceType> getter;
        private readonly PropertyAccessor.Setter<TTarget, TTargetType> setter;
        private readonly TypeConverter typeConverter;

        public TypeConverterPropertyMapper(PropertyInfo sourceProperty, PropertyInfo targetProperty, TypeConverter typeConverter)
        {
            getter = PropertyAccessor.CreateGetter<TSource, TSourceType>(sourceProperty);
            setter = PropertyAccessor.CreateSetter<TTarget, TTargetType>(targetProperty);

            this.typeConverter = typeConverter;
        }

        public void MapProperty(TSource source, TTarget target, ref MappingContext context)
        {
            var value = (object?)getter(source);

            if (value == null)
            {
                return;
            }

            try
            {
                var converted = typeConverter.ConvertFrom(null, context.Culture, value);

                setter(target, (TTargetType)converted!);
            }
            catch
            {
                return;
            }
        }
    }

    private static class ClassMapper<TSource, TTarget> where TSource : class where TTarget : class
    {
        private static readonly List<IPropertyMapper<TSource, TTarget>> Mappers = [];

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
                    Mappers.Add(SimplePropertyMapper.Create<TSource, TTarget>(
                        sourceType,
                        sourceProperty,
                        targetProperty));
                }
                else if (targetType == typeof(string))
                {
                    Mappers.Add(StringConversionPropertyMapper.Create<TSource, TTarget>(
                        sourceType,
                        sourceProperty,
                        targetProperty));
                }
                else if (IsNullableOf(sourceType, targetType))
                {
                    Mappers.Add(NullablePropertyMapper.Create<TSource, TTarget>(
                        targetType,
                        sourceProperty,
                        targetProperty));
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(targetType);

                    if (converter.CanConvertFrom(sourceType))
                    {
                        Mappers.Add(TypeConverterPropertyMapper.Create<TSource, TTarget>(
                            sourceType,
                            sourceProperty,
                            targetType,
                            targetProperty,
                            converter));
                    }
                    else if (sourceType.Implements<IConvertible>() || targetType.Implements<IConvertible>())
                    {
                        Mappers.Add(ConversionPropertyMapper.Create<TSource, TTarget>(
                            sourceType,
                            sourceProperty,
                            targetType,
                            targetProperty));
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
            var count = Mappers.Count;

            for (var i = 0; i < count; i++)
            {
                Mappers[i].MapProperty(source, destination, ref context);
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
