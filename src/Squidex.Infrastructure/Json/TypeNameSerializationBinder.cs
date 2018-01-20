// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json.Serialization;

namespace Squidex.Infrastructure.Json
{
    public class TypeNameSerializationBinder : DefaultSerializationBinder
    {
        private readonly TypeNameRegistry typeNameRegistry;

        public TypeNameSerializationBinder(TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.typeNameRegistry = typeNameRegistry;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            var type = typeNameRegistry.GetTypeOrNull(typeName);

            return type ?? base.BindToType(assemblyName, typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;

            var name = typeNameRegistry.GetNameOrNull(serializedType);

            if (name != null)
            {
                typeName = name;
            }
            else
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }
    }
}