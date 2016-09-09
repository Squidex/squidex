// ==========================================================================
//  ModelField_Generic.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure;

namespace PinkParrot.Core.Schema
{
    public abstract class ModelField<T> : ModelField where T : ModelFieldProperties
    {
        private T properties;

        public override ModelFieldProperties RawProperties
        {
            get { return properties; }
        }

        public override string Name
        {
            get { return properties.Name; }
        }

        public override string Label
        {
            get { return properties.Label ?? properties.Name; }
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

        protected ModelField(long id, T properties) 
            : base(id)
        {
            Guard.NotNull(properties, nameof(properties));

            this.properties = properties;
        }

        public override ModelField Configure(ModelFieldProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            var typedProperties = newProperties as T;

            if (typedProperties == null)
            {
                throw new ArgumentException($"Properties must be of type '{typeof(T)}", nameof(newProperties));
            }

            return Update<ModelField<T>>(clone => clone.properties = typedProperties);
        }
    }
}
