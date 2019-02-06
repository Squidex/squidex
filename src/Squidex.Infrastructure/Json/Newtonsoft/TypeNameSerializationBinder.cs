// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class TypeNameSerializationBinder : ISerializationBinder
    {
        private readonly TypeNameRegistry typeNameRegistry;

        public TypeNameSerializationBinder(TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.typeNameRegistry = typeNameRegistry;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            return typeNameRegistry.GetTypeOrNull(typeName);
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;

            typeName = typeNameRegistry.GetNameOrNull(serializedType);

            if (typeName == null)
            {
                throw new JsonException("Trying to serialize object with type name.");
            }
        }
    }
}