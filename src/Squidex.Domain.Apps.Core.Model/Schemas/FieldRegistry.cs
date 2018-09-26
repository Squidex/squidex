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
        private readonly TypeNameRegistry typeNameRegistry;
        private readonly HashSet<Type> supportedFields = new HashSet<Type>();

        public FieldRegistry(TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.typeNameRegistry = typeNameRegistry;

            var types = typeof(FieldRegistry).Assembly.GetTypes().Where(x => x.BaseType == typeof(FieldProperties));

            foreach (var type in types)
            {
                RegisterField(type);
            }

            typeNameRegistry.MapObsolete(typeof(ReferencesFieldProperties), "References");
            typeNameRegistry.MapObsolete(typeof(DateTimeFieldProperties), "DateTime");
        }

        private void RegisterField(Type type)
        {
            if (supportedFields.Add(type))
            {
                typeNameRegistry.Map(type);
            }
        }

        public RootField CreateRootField(long id, string name, Partitioning partitioning, FieldProperties properties)
        {
            CheckProperties(properties);

            return properties.CreateRootField(id, name, partitioning);
        }

        public NestedField CreateNestedField(long id, string name, FieldProperties properties)
        {
            CheckProperties(properties);

            return properties.CreateNestedField(id, name);
        }

        private void CheckProperties(FieldProperties properties)
        {
            Guard.NotNull(properties, nameof(properties));

            if (!supportedFields.Contains(properties.GetType()))
            {
                throw new InvalidOperationException($"The field property '{properties.GetType()}' is not supported.");
            }
        }
    }
}
