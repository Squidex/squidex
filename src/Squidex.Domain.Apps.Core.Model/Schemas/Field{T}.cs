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
    public abstract class Field<T> : Field where T : FieldProperties, new()
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

        protected Field(long id, string name, Partitioning partitioning, T properties)
            : base(id, name, partitioning)
        {
            Guard.NotNull(properties, nameof(properties));

            this.properties = properties;
            this.properties.Freeze();
        }

        [Pure]
        public override Field Update(FieldProperties newProperties)
        {
            var typedProperties = ValidateProperties(newProperties);

            return Clone<Field<T>>(clone =>
            {
                clone.properties = typedProperties;
                clone.properties.Freeze();
            });
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
    }
}
