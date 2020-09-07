// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentDataFlatGraphType : ObjectGraphType<FlatContentData>
    {
        public ContentDataFlatGraphType(ISchemaEntity schema, string schemaName, string schemaType, IGraphModel model)
        {
            Name = $"{schemaType}DataFlatDto";

            foreach (var (field, fieldName, _) in schema.SchemaDef.Fields.SafeFields())
            {
                var (resolvedType, valueResolver, args) = model.GetGraphType(schema, field, fieldName);

                if (valueResolver != null)
                {
                    AddField(new FieldType
                    {
                        Name = fieldName,
                        Arguments = args,
                        Resolver = ContentResolvers.FlatPartition(valueResolver, field.Name),
                        ResolvedType = resolvedType,
                        Description = field.RawProperties.Hints
                    });
                }
            }

            Description = $"The structure of the flat {schemaName} data type.";
        }
    }
}
