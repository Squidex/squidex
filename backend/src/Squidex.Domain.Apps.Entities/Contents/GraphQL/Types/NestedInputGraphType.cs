// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class NestedInputGraphType : InputObjectGraphType
    {
        public NestedInputGraphType(IGraphModel model, ISchemaEntity schema, IArrayField field, string fieldName)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            var fieldDisplayName = field.DisplayName();

            Name = $"{schemaType}{fieldName}ChildDto";

            foreach (var (nestedField, nestedName, _) in field.Fields.SafeFields().Where(x => x.Field.IsForApi(true)))
            {
                var resolvedType = model.GetInputGraphType(schema, nestedField, nestedName);

                if (resolvedType != null)
                {
                    if (field.RawProperties.IsRequired)
                    {
                        resolvedType = new NonNullGraphType(resolvedType);
                    }

                    AddField(new FieldType
                    {
                        Name = nestedName,
                        Resolver = null,
                        ResolvedType = resolvedType,
                        Description = $"The {fieldDisplayName}/{nestedField.DisplayName()} nested field."
                    });
                }
            }

            Description = $"The structure of the {schemaName}.{fieldDisplayName} nested schema.";
        }
    }
}
