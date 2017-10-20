// ==========================================================================
//  Field_Generic.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;
namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class Field<T> : Field where T : FieldProperties
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

            this.properties = ValidateProperties(properties);
        }

        protected override Field UpdateInternal(FieldProperties newProperties)
        {
            var typedProperties = ValidateProperties(newProperties);

            return Clone<Field<T>>(clone => clone.properties = typedProperties);
        }

        private T ValidateProperties(FieldProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            newProperties.Freeze();

            if (!(newProperties is T typedProperties))
            {
                throw new ArgumentException($"Properties must be of type '{typeof(T)}", nameof(newProperties));
            }

            return typedProperties;
        }
    }
}
