// ==========================================================================
//  ModelFieldFactory.cs
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
    public class ModelFieldFactory
    {
        private readonly Dictionary<Type, Func<long, ModelFieldProperties, ModelField>> factories 
            = new Dictionary<Type, Func<long, ModelFieldProperties, ModelField>>();

        public ModelFieldFactory()
        {
            AddFactory<NumberFieldProperties>((id, p) => new NumberField(id, (NumberFieldProperties)p));
        }

        public ModelFieldFactory AddFactory<T>(Func<long, ModelFieldProperties, ModelField> factory) where T : ModelFieldProperties
        {
            Guard.NotNull(factory, nameof(factory));

            factories[typeof(T)] = factory;

            return this;
        }

        public virtual ModelField CreateField(long id, ModelFieldProperties properties)
        {
            Guard.NotNull(properties, nameof(properties));

            var factory = factories.GetOrDefault(properties.GetType());

            if (factory == null)
            {
                throw new InvalidOperationException("Field type is not supported.");
            }

            return factory(id, properties);
        }
    }
}

