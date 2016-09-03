// ==========================================================================
//  ModelField_Generic.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
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
            get { return properties.Label; }
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

        protected ModelField(long id) 
            : base(id)
        {
        }

        public override ModelField Configure(ModelFieldProperties newProperties, IList<ValidationError> errors)
        {
            Guard.NotNull(newProperties, nameof(newProperties));
            Guard.NotNull(errors, nameof(errors));

            var typedProperties = newProperties as T;

            if (typedProperties == null)
            {
                throw new ArgumentException($"Properties must be of type '{typeof(T)}", nameof(newProperties));
            }

            newProperties.Validate(errors);

            return Update<ModelField<T>>(clone => clone.properties = typedProperties);
        }
    }
}
