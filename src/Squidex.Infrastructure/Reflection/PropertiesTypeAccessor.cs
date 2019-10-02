﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Squidex.Infrastructure.Reflection
{
    public sealed class PropertiesTypeAccessor
    {
        private static readonly ConcurrentDictionary<Type, PropertiesTypeAccessor> AccessorCache = new ConcurrentDictionary<Type, PropertiesTypeAccessor>();
        private readonly Dictionary<string, IPropertyAccessor> accessors = new Dictionary<string, IPropertyAccessor>();
        private readonly List<PropertyInfo> properties = new List<PropertyInfo>();

        public IEnumerable<PropertyInfo> Properties
        {
            get
            {
                return properties;
            }
        }

        private PropertiesTypeAccessor(Type type)
        {
            var allProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in allProperties)
            {
                accessors[property.Name] = new PropertyAccessor(type, property);

                properties.Add(property);
            }
        }

        public static PropertiesTypeAccessor Create(Type targetType)
        {
            Guard.NotNull(targetType);

            return AccessorCache.GetOrAdd(targetType, x => new PropertiesTypeAccessor(x));
        }

        public void SetValue(object target, string propertyName, object? value)
        {
            Guard.NotNull(target);

            var accessor = FindAccessor(propertyName);

            accessor.Set(target, value);
        }

        public object? GetValue(object target, string propertyName)
        {
            Guard.NotNull(target);

            var accessor = FindAccessor(propertyName);

            return accessor.Get(target);
        }

        private IPropertyAccessor FindAccessor(string propertyName)
        {
            Guard.NotNullOrEmpty(propertyName);

            if (!accessors.TryGetValue(propertyName, out var accessor))
            {
                throw new ArgumentException("Property does not exist.", nameof(propertyName));
            }

            return accessor;
        }
    }
}
