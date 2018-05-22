// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class FieldRegistry
    {
        private delegate Field FactoryFunction(long id, string name, Partitioning partitioning, FieldProperties properties);

        private readonly TypeNameRegistry typeNameRegistry;
        private readonly Dictionary<Type, FactoryFunction> fieldsByPropertyType = new Dictionary<Type, FactoryFunction>();

        public FieldRegistry(TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.typeNameRegistry = typeNameRegistry;

            var types = typeof(FieldRegistry).Assembly.GetTypes().Where(x => x.BaseType == typeof(FieldProperties));

            foreach (var type in types)
            {
                RegisterField(type);
            }

            typeNameRegistry.MapObsolete(typeof(ReferencesFieldProperties), "DateTime");
            typeNameRegistry.MapObsolete(typeof(DateTimeFieldProperties), "References");
        }

        private void RegisterField(Type type)
        {
            typeNameRegistry.Map(type);

            fieldsByPropertyType[type] = (id, name, partitioning, properties) => properties.CreateField(id, name, partitioning);
        }

        public Field CreateField(long id, string name, Partitioning partitioning, FieldProperties properties)
        {
            Guard.NotNull(properties, nameof(properties));

            var factory = fieldsByPropertyType.GetOrDefault(properties.GetType());

            if (factory == null)
            {
                throw new InvalidOperationException($"The field property '{properties.GetType()}' is not supported.");
            }

            return factory(id, name, partitioning, properties);
        }
    }
}
