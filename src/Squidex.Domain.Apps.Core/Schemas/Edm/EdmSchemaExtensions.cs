// ==========================================================================
//  EdmSchemaExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas.Edm
{
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

        public static EdmComplexType BuildEdmType(this Schema schema, PartitionResolver partitionResolver, Func<EdmComplexType, EdmComplexType> typeResolver)
        {
            Guard.NotNull(typeResolver, nameof(typeResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var schemaName = schema.Name.ToPascalCase();

            var edmType = new EdmComplexType("Squidex", schemaName);

            foreach (var field in schema.FieldsByName.Values.Where(x => !x.IsHidden))
            {
                var edmValueType = EdmTypeVisitor.CreateEdmType(field);

                if (edmValueType == null)
                {
                    continue;
                }

                var partitionType = typeResolver(new EdmComplexType("Squidex", $"{schemaName}{field.Name.ToPascalCase()}Property"));
                var partition = partitionResolver(field.Partitioning);

                foreach (var partitionItem in partition)
                {
                    partitionType.AddStructuralProperty(partitionItem.Key, edmValueType);
                }

                edmType.AddStructuralProperty(field.Name.EscapeEdmField(), new EdmComplexTypeReference(partitionType, false));
            }

            return edmType;
        }
    }
}
