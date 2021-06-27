// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json.Serialization;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class TypeNameSerializationBinder : DefaultSerializationBinder
    {
        private readonly TypeNameRegistry typeNameRegistry;

        public TypeNameSerializationBinder(TypeNameRegistry typeNameRegistry)
        {
            this.typeNameRegistry = typeNameRegistry;
        }

        public override Type BindToType(string? assemblyName, string typeName)
        {
            var type = typeNameRegistry.GetTypeOrNull(typeName);

            return type ?? base.BindToType(assemblyName, typeName);
        }

        public override void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
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