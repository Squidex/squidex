﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class NestedField<T> : NestedField, IField<T> where T : FieldProperties, new()
    {
        private T properties;

        public T Properties
        {
            get => properties;
        }

        public override FieldProperties RawProperties
        {
            get => properties;
        }

        public NestedField(long id, string name, T? properties = null, IFieldSettings? settings = null)
            : base(id, name, settings)
        {
            SetProperties(properties ?? new T());
        }

        [Pure]
        public override NestedField Update(FieldProperties newProperties)
        {
            var typedProperties = ValidateProperties(newProperties);

            typedProperties.Freeze();

            if (properties.Equals(typedProperties))
            {
                return this;
            }

            return Clone<NestedField<T>>(clone =>
            {
                clone.SetProperties(typedProperties);
            });
        }

        private void SetProperties(T newProperties)
        {
            properties = newProperties;
            properties.Freeze();
        }

        private static T ValidateProperties(FieldProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            if (newProperties is not T typedProperties)
            {
                throw new ArgumentException($"Properties must be of type '{typeof(T)}", nameof(newProperties));
            }

            return typedProperties;
        }

        public override TResult Accept<TResult, TArgs>(IFieldVisitor<TResult, TArgs> visitor, TArgs args)
        {
            return properties.Accept(visitor, this, args);
        }
    }
}
