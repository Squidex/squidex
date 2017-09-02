// ==========================================================================
//  PropertiesBag.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Squidex.Infrastructure
{
    public class PropertiesBag : DynamicObject
    {
        private static readonly PropertyValue FallbackValue = new PropertyValue(null);
        private readonly Dictionary<string, PropertyValue> internalDictionary = new Dictionary<string, PropertyValue>(StringComparer.OrdinalIgnoreCase);

        public int Count
        {
            get { return internalDictionary.Count; }
        }

        public IReadOnlyDictionary<string, PropertyValue> Properties
        {
            get { return internalDictionary; }
        }

        public IEnumerable<string> PropertyNames
        {
            get { return internalDictionary.Keys; }
        }

        public PropertyValue this[string propertyName]
        {
            get
            {
                Guard.NotNullOrEmpty(propertyName, nameof(propertyName));

                return internalDictionary.GetOrDefault(propertyName) ?? FallbackValue;
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return internalDictionary.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            internalDictionary[binder.Name] = new PropertyValue(value);

            return true;
        }

        public bool Contains(string propertyName)
        {
            Guard.NotNullOrEmpty(propertyName, nameof(propertyName));

            return internalDictionary.ContainsKey(propertyName);
        }

        public bool Remove(string propertyName)
        {
            Guard.NotNullOrEmpty(propertyName, nameof(propertyName));

            return internalDictionary.Remove(propertyName);
        }

        public PropertiesBag Set(string propertyName, object value)
        {
            Guard.NotNullOrEmpty(propertyName, nameof(propertyName));

            internalDictionary[propertyName] = new PropertyValue(value);

            return this;
        }

        public bool Rename(string oldPropertyName, string newPropertyName)
        {
            Guard.NotNullOrEmpty(oldPropertyName, nameof(oldPropertyName));
            Guard.NotNullOrEmpty(newPropertyName, nameof(newPropertyName));

            if (internalDictionary.ContainsKey(newPropertyName))
            {
                throw new ArgumentException($"An property with the key '{newPropertyName}' already exists.", newPropertyName);
            }

            if (string.Equals(oldPropertyName, newPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"The property names '{newPropertyName}' are equal.", newPropertyName);
            }

            if (!internalDictionary.TryGetValue(oldPropertyName, out var property))
            {
                return false;
            }

            internalDictionary[newPropertyName] = property;
            internalDictionary.Remove(oldPropertyName);

            return true;
        }
    }
}
