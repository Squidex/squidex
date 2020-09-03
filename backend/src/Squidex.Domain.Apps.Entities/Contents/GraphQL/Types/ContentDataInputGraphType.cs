// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentDataInputGraphType : InputObjectGraphType
    {
        public ContentDataInputGraphType(ISchemaEntity schema, string schemaName, string schemaType, IGraphModel model)
        {
            Name = $"{schemaType}DataInputDto";

            foreach (var (field, fieldName, typeName) in schema.SchemaDef.Fields.SafeFields().Where(x => x.Field.IsForApi(true)))
            {
                var resolvedType = model.GetInputGraphType(schema, field, typeName);

                if (resolvedType != null)
                {
                    var displayName = field.DisplayName();

                    var fieldGraphType = new InputObjectGraphType
                    {
                        Name = $"{schemaType}Data{typeName}InputDto"
                    };

                    var partitioning = model.ResolvePartition(field.Partitioning);

                    foreach (var partitionKey in partitioning.AllKeys)
                    {
                        fieldGraphType.AddField(new FieldType
                        {
                            Name = partitionKey.EscapePartition(),
                            Resolver = null,
                            ResolvedType = resolvedType,
                            Description = field.RawProperties.Hints
                        }).WithSourceName( partitionKey);
                    }

                    fieldGraphType.Description = $"The structure of the {displayName} field of the {schemaName} content input type.";

                    AddField(new FieldType
                    {
                        Name = fieldName,
                        Resolver = null,
                        ResolvedType = fieldGraphType,
                        Description = $"The {displayName} field."
                    }).WithSourceName(field.Name);
                }
            }

            Description = $"The structure of the {schemaName} data input type.";
        }
    }
}
