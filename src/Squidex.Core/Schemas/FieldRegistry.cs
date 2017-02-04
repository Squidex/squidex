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

namespace Squidex.Core.Schemas
{
    public delegate Field FactoryFunction(long id, string name, FieldProperties properties);

    public sealed class FieldRegistry
    {
        private readonly TypeNameRegistry typeNameRegistry;
        private readonly Dictionary<string, IRegisteredField> fieldsByTypeName = new Dictionary<string, IRegisteredField>();
        private readonly Dictionary<Type, IRegisteredField> fieldsByPropertyType = new Dictionary<Type, IRegisteredField>();

        private sealed class Registered : IRegisteredField
        {
            private readonly FactoryFunction fieldFactory;
            private readonly Type propertiesType;
            private readonly string typeName;

            public Type PropertiesType
            {
                get { return propertiesType; }
            }

            public string TypeName
            {
                get { return typeName; }
            }

            public Registered(FactoryFunction fieldFactory, Type propertiesType, TypeNameRegistry typeNameRegistry)
            {
                typeName = typeNameRegistry.GetName(propertiesType);

                this.fieldFactory = fieldFactory;
                this.propertiesType = propertiesType;
            }

            Field IRegisteredField.CreateField(long id, string name, FieldProperties properties)
            {
                return fieldFactory(id, name, properties);
            }
        }

        public FieldRegistry(TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.typeNameRegistry = typeNameRegistry;

            Add<BooleanFieldProperties>(
                (id, name, p) => new BooleanField(id, name, (BooleanFieldProperties)p));

            Add<NumberFieldProperties>(
                (id, name, p) => new NumberField(id, name, (NumberFieldProperties)p));

            Add<StringFieldProperties>(
                (id, name, p) => new StringField(id, name, (StringFieldProperties)p));
        }

        public void Add<TFieldProperties>(FactoryFunction fieldFactory)
        {
            Guard.NotNull(fieldFactory, nameof(fieldFactory));

            typeNameRegistry.Map(typeof(TFieldProperties));
           
            var registered = new Registered(fieldFactory, typeof(TFieldProperties), typeNameRegistry);

            fieldsByTypeName[registered.TypeName] = registered;
            fieldsByPropertyType[registered.PropertiesType] = registered;
        }

        public Field CreateField(long id, string name, FieldProperties properties)
        {
            var registered = fieldsByPropertyType.GetOrDefault(properties.GetType());

            if (registered == null)
            {
                throw new InvalidOperationException($"The field property '{properties.GetType()}' is not supported.");
            }

            return registered.CreateField(id, name, properties);
        }

        public IRegisteredField FindByPropertiesType(Type type)
        {
            Guard.NotNull(type, nameof(type));

            var registered = fieldsByPropertyType.GetOrDefault(type);

            if (registered == null)
            {
                throw new InvalidOperationException($"The field property '{type}' is not supported.");
            }

            return registered;
        }

        public IRegisteredField FindByTypeName(string typeName)
        {
            Guard.NotNullOrEmpty(typeName, nameof(typeName));

            var registered = fieldsByTypeName.GetOrDefault(typeName);

            if (registered == null)
            {
                throw new DomainException($"A field with type '{typeName} is not known.");
            }

            return registered;
        }
    }
}
