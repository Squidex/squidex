// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure.Reflection.Equality;
using Squidex.Infrastructure.Reflection.Internal;

namespace Squidex.Infrastructure.Reflection
{
    public static class SimpleEquals
    {
        private static readonly ConcurrentDictionary<Type, IDeepComparer?> Comparers = new ConcurrentDictionary<Type, IDeepComparer?>();
        private static readonly HashSet<Type> SimpleTypes = new HashSet<Type>();
        private static readonly DefaultComparer DefaultComparer = new DefaultComparer();
        private static readonly NoopComparer NoopComparer = new NoopComparer();

        static SimpleEquals()
        {
            SimpleTypes.Add(typeof(string));
            SimpleTypes.Add(typeof(Uri));
        }

        internal static IDeepComparer Build(Type type)
        {
            return BuildCore(type) ?? DefaultComparer;
        }

        internal static IDeepComparer BuildInner(Type type)
        {
            return BuildCore(type) ?? NoopComparer;
        }

        private static IDeepComparer? BuildCore(Type t)
        {
            return Comparers.GetOrAdd(t, type =>
            {
                if (IsSimpleType(type) || IsEquatable(type))
                {
                    return null;
                }

                if (IsArray(type))
                {
                    var comparerType = typeof(ArrayComparer<>).MakeGenericType(type.GetElementType()!);

                    return (IDeepComparer)Activator.CreateInstance(comparerType, DefaultComparer)!;
                }

                if (IsSet(type))
                {
                    var comparerType = typeof(SetComparer<>).MakeGenericType(type.GetGenericArguments());

                    return (IDeepComparer)Activator.CreateInstance(comparerType, DefaultComparer)!;
                }

                if (IsDictionary(type))
                {
                    var comparerType = typeof(DictionaryComparer<,>).MakeGenericType(type.GetGenericArguments());

                    return (IDeepComparer)Activator.CreateInstance(comparerType, DefaultComparer)!;
                }

                if (IsCollection(type))
                {
                    PropertyAccessor? count = null;

                    var countProperty = type.GetProperty("Count");

                    if (countProperty != null && countProperty.PropertyType == typeof(int))
                    {
                        count = new PropertyAccessor(type, countProperty);
                    }

                    return (IDeepComparer)Activator.CreateInstance(typeof(CollectionComparer), DefaultComparer, count)!;
                }

                return new ObjectComparer(DefaultComparer, type);
            });
        }

        private static bool IsArray(Type type)
        {
            return type.IsArray;
        }

        private static bool IsCollection(Type type)
        {
            return type.GetInterfaces().Contains(typeof(IEnumerable));
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsValueType || SimpleTypes.Contains(type);
        }

        private static bool IsEquatable(Type type)
        {
            return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEquatable<>));
        }

        private static bool IsSet(Type type)
        {
            return
                type.GetGenericArguments().Length == 1 &&
                type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISet<>));
        }

        private static bool IsDictionary(Type type)
        {
            return
                type.GetGenericArguments().Length == 2 &&
                type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>));
        }

        public static bool IsEquals<T>(T x, T y)
        {
            return DefaultComparer.IsEquals(x, y);
        }
    }
}
