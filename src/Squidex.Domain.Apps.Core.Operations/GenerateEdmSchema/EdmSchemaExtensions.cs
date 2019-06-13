﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.GenerateEdmSchema
{
    public delegate (EdmComplexType Type, bool Created) EdmTypeFactory(string names);

    public static class EdmSchemaExtensions
    {
        public static string EscapeEdmField(this string field)
        {
            return field.Replace("-", "_");
        }

        public static string UnescapeEdmField(this string field)
        {
            return field.Replace("_", "-");
        }

        public static EdmComplexType BuildEdmType(this Schema schema, bool withHidden, PartitionResolver partitionResolver, EdmTypeFactory typeFactory)
        {
            Guard.NotNull(typeFactory, nameof(typeFactory));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var (edmType, _) = typeFactory("Data");

            var visitor = new EdmTypeVisitor(typeFactory);

            foreach (var field in schema.FieldsByName.Values)
            {
                if (!field.IsForApi(withHidden))
                {
                    continue;
                }

                var fieldEdmType = field.Accept(visitor);

                if (fieldEdmType == null)
                {
                    continue;
                }

                var (partitionType, created) = typeFactory($"Data.{field.Name.ToPascalCase()}");

                if (created)
                {
                    var partition = partitionResolver(field.Partitioning);

                    foreach (var partitionItem in partition)
                    {
                        partitionType.AddStructuralProperty(partitionItem.Key.EscapeEdmField(), fieldEdmType);
                    }
                }

                edmType.AddStructuralProperty(field.Name.EscapeEdmField(), new EdmComplexTypeReference(partitionType, false));
            }

            return edmType;
        }
    }
}
