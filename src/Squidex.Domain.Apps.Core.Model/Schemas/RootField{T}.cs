// ==========================================================================
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
    public class RootField<T> : RootField, IField<T> where T : FieldProperties, new()
    {
        private T properties;

        public T Properties
        {
            get { return properties; }
        }

        public override FieldProperties RawProperties
        {
            get { return properties; }
        }

        public RootField(long id, string name, Partitioning partitioning, T properties)
            : base(id, name, partitioning)
        {
            Guard.NotNull(properties, nameof(properties));

            SetProperties(properties);
        }

        [Pure]
        public override RootField Update(FieldProperties newProperties)
        {
            var typedProperties = ValidateProperties(newProperties);

            return Clone<RootField<T>>(clone =>
            {
                clone.SetProperties(typedProperties);
            });
        }

        private void SetProperties(T newProperties)
        {
            properties = newProperties;
            properties.Freeze();
        }

        private T ValidateProperties(FieldProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            if (!(newProperties is T typedProperties))
            {
                throw new ArgumentException($"Properties must be of type '{typeof(T)}", nameof(newProperties));
            }

            return typedProperties;
        }

        public override TResult Accept<TResult>(IFieldVisitor<TResult> visitor)
        {
            return properties.Accept(visitor, this);
        }
    }
}
