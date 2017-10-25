// ==========================================================================
//  FieldRegistry.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
        private readonly Dictionary<Type, Registered> fieldsByPropertyType = new Dictionary<Type, Registered>();

        private sealed class Registered
        {
            private readonly FactoryFunction fieldFactory;
            private readonly Type propertiesType;

            public Type PropertiesType
            {
                get { return propertiesType; }
            }

            public Registered(FactoryFunction fieldFactory, Type propertiesType)
            {
                this.fieldFactory = fieldFactory;
                this.propertiesType = propertiesType;
            }

            public Field CreateField(long id, string name, Partitioning partitioning, FieldProperties properties)
            {
                return fieldFactory(id, name, partitioning, properties);
            }
        }

        public FieldRegistry(TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.typeNameRegistry = typeNameRegistry;

            Add<BooleanFieldProperties>(
                (id, name, partitioning, properties) =>
                    new BooleanField(id, name, partitioning, (BooleanFieldProperties)properties));

            Add<NumberFieldProperties>(
                (id, name, partitioning, properties) =>
                    new NumberField(id, name, partitioning, (NumberFieldProperties)properties));

            Add<StringFieldProperties>(
                (id, name, partitioning, properties) =>
                    new StringField(id, name, partitioning, (StringFieldProperties)properties));

            Add<JsonFieldProperties>(
                (id, name, partitioning, properties) =>
                    new JsonField(id, name, partitioning, (JsonFieldProperties)properties));

            Add<AssetsFieldProperties>(
                (id, name, partitioning, properties) =>
                    new AssetsField(id, name, partitioning, (AssetsFieldProperties)properties));

            Add<GeolocationFieldProperties>(
                (id, name, partitioning, properties) =>
                    new GeolocationField(id, name, partitioning, (GeolocationFieldProperties)properties));

            Add<ReferencesFieldProperties>(
                (id, name, partitioning, properties) =>
                    new ReferencesField(id, name, partitioning, (ReferencesFieldProperties)properties));

            Add<DateTimeFieldProperties>(
                (id, name, partitioning, properties) =>
                    new DateTimeField(id, name, partitioning, (DateTimeFieldProperties)properties));

            Add<TagsFieldProperties>(
                (id, name, partitioning, properties) =>
                    new TagsField(id, name, partitioning, (TagsFieldProperties)properties));

            typeNameRegistry.MapObsolete(typeof(ReferencesFieldProperties), "DateTime");

            typeNameRegistry.MapObsolete(typeof(DateTimeFieldProperties), "References");
        }

        private void Add<TFieldProperties>(FactoryFunction fieldFactory)
        {
            Guard.NotNull(fieldFactory, nameof(fieldFactory));

            typeNameRegistry.Map(typeof(TFieldProperties));

            var registered = new Registered(fieldFactory, typeof(TFieldProperties));

            fieldsByPropertyType[registered.PropertiesType] = registered;
        }

        public Field CreateField(long id, string name, Partitioning partitioning, FieldProperties properties)
        {
            Guard.NotNull(properties, nameof(properties));

            var registered = fieldsByPropertyType.GetOrDefault(properties.GetType());

            if (registered == null)
            {
                throw new InvalidOperationException($"The field property '{properties.GetType()}' is not supported.");
            }

            return registered.CreateField(id, name, partitioning, properties);
        }
    }
}
