// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.OData.Edm;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.GenerateEdmSchema
{
    public delegate (EdmComplexType Type, bool Created) EdmTypeFactory(string name);

    public static class EdmSchemaExtensions
    {
        public static EdmComplexType BuildEdmType(this Schema schema, bool withHidden, PartitionResolver partitionResolver, EdmTypeFactory factory,
            ResolvedComponents components)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var (edmType, _) = factory("Data");

            foreach (var field in schema.FieldsByName.Values)
            {
                if (!field.IsForApi(withHidden))
                {
                    continue;
                }

                var fieldEdmType = EdmTypeVisitor.BuildType(field, factory, components);

                if (fieldEdmType == null)
                {
                    continue;
                }

                var (partitionType, created) = factory($"Data.{field.Name.ToPascalCase()}");

                if (created)
                {
                    var partitioning = partitionResolver(field.Partitioning);

                    foreach (var partitionKey in partitioning.AllKeys)
                    {
                        partitionType.AddStructuralProperty(partitionKey.EscapeEdmField(), fieldEdmType);
                    }
                }

                edmType.AddStructuralProperty(field.Name.EscapeEdmField(), new EdmComplexTypeReference(partitionType, false));
            }

            return edmType;
        }

        public static string EscapeEdmField(this string field)
        {
            return field.Replace("-", "_", StringComparison.Ordinal);
        }

        public static string UnescapeEdmField(this string field)
        {
            return field.Replace("_", "-", StringComparison.Ordinal);
        }
    }
}
