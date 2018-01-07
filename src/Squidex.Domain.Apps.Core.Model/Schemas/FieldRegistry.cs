// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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

            RegisterField<AssetsFieldProperties>();
            RegisterField<BooleanFieldProperties>();
            RegisterField<DateTimeFieldProperties>();
            RegisterField<GeolocationFieldProperties>();
            RegisterField<JsonFieldProperties>();
            RegisterField<NumberFieldProperties>();
            RegisterField<ReferencesFieldProperties>();
            RegisterField<StringFieldProperties>();
            RegisterField<TagsFieldProperties>();

            typeNameRegistry.MapObsolete(typeof(ReferencesFieldProperties), "DateTime");
            typeNameRegistry.MapObsolete(typeof(DateTimeFieldProperties), "References");
        }

        private void RegisterField<T>()
        {
            typeNameRegistry.Map(typeof(T));

            fieldsByPropertyType[typeof(T)] = (id, name, partitioning, properties) => properties.CreateField(id, name, partitioning);
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
