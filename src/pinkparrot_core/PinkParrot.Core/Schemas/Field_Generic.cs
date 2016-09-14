// ==========================================================================
//  Field_Generic.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure;

namespace PinkParrot.Core.Schemas
{
    public abstract class Field<T> : Field where T : FieldProperties
    {
        private T properties;

        public override IFieldProperties RawProperties
        {
            get { return properties; }
        }

        public override string Label
        {
            get { return properties.Label ?? Name; }
        }

        public override string Hints
        {
            get { return properties.Hints; }
        }

        public override bool IsRequired
        {
            get { return properties.IsRequired; }
        }

        public T Properties
        {
            get { return properties; }
        }

        protected Field(long id, string name, T properties) 
            : base(id, name)
        {
            Guard.NotNull(properties, nameof(properties));

            this.properties = properties;
        }

        public override Field Update(IFieldProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));
            
            var typedProperties = newProperties as T;

            if (typedProperties == null)
            {
                throw new ArgumentException($"Properties must be of type '{typeof(T)}", nameof(newProperties));
            }

            newProperties.Validate(() => $"Cannot update field with id '{Id}', becase the settings are invalid.");

            return Update<Field<T>>(clone => clone.properties = typedProperties);
        }
    }
}
