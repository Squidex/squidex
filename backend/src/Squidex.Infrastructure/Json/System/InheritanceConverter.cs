// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Json.System
{
    public sealed class InheritanceConverter<T> : InheritanceConverterBase<T> where T : notnull
    {
        private readonly TypeNameRegistry typeNameRegistry;

        public InheritanceConverter(TypeNameRegistry typeNameRegistry)
            : base("$type")
        {
            this.typeNameRegistry = typeNameRegistry;
        }

        public override Type GetDiscriminatorType(string name, Type typeToConvert)
        {
            var typeInfo = typeNameRegistry.GetTypeOrNull(name);

            if (typeInfo == null)
            {
                typeInfo = Type.GetType(name);
            }

            if (typeInfo == null)
            {
                ThrowHelper.JsonException($"Object has invalid discriminator '{name}'.");
                return default!;
            }

            return typeInfo;
        }

        public override string GetDiscriminatorValue(Type type)
        {
            var typeName = typeNameRegistry.GetNameOrNull(type);

            if (typeName == null)
            {
                // Use the type name as a fallback.
                typeName = type.AssemblyQualifiedName!;
            }

            return typeName;
        }
    }
}
