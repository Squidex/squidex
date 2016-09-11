// ==========================================================================
//  ModelFieldRegistry.cs
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
    public delegate ModelField FactoryFunction(long id, string name, IModelFieldProperties properties);

    public sealed class ModelFieldRegistry
    {
        private readonly Dictionary<string, IRegisterModelField> fieldsByTypeName = new Dictionary<string, IRegisterModelField>();
        private readonly Dictionary<Type, IRegisterModelField> fieldsByPropertyType = new Dictionary<Type, IRegisterModelField>();

        private sealed class Registered : IRegisterModelField
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

            public Registered(FactoryFunction fieldFactory, Type propertiesType)
            {
                typeName = TypeNameRegistry.GetName(propertiesType);

                this.fieldFactory = fieldFactory;
                this.propertiesType = propertiesType;
            }

            ModelField IRegisterModelField.CreateField(long id, string name, IModelFieldProperties properties)
            {
                return fieldFactory(id, name, properties);
            }
        }

        public void Add<TFieldProperties>(FactoryFunction fieldFactory)
        {
            Guard.NotNull(fieldFactory, nameof(fieldFactory));
           
            var registered = new Registered(fieldFactory, typeof(TFieldProperties));

            fieldsByTypeName[registered.TypeName] = registered;
            fieldsByPropertyType[registered.PropertiesType] = registered;
        }

        public ModelField CreateField(long id, string name, IModelFieldProperties properties)
        {
            var registered = fieldsByPropertyType[properties.GetType()];

            return registered.CreateField(id, name, properties);
        }

        public IRegisterModelField FindByPropertiesType(Type type)
        {
            Guard.NotNull(type, nameof(type));

            var registered = fieldsByPropertyType.GetOrDefault(type);

            if (registered == null)
            {
                throw new InvalidOperationException($"The field property '{type}' is not supported.");
            }

            return registered;
        }

        public IRegisterModelField FindByTypeName(string typeName)
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
