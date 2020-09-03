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
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class NestedGraphType : ObjectGraphType<JsonObject>
    {
        public NestedGraphType(IGraphModel model, ISchemaEntity schema, IArrayField field, string fieldName)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            var fieldDisplayName = field.DisplayName();

            Name = $"{schemaType}{fieldName}ChildDto";

            foreach (var (nestedField, nestedName, typeName) in field.Fields.SafeFields().Where(x => x.Field.IsForApi()))
            {
                var (resolvedType, valueResolver, args) = model.GetGraphType(schema, nestedField, typeName);

                if (resolvedType != null && valueResolver != null)
                {
                    var resolver = ContentResolvers.NestedValue(valueResolver, nestedField.Name);

                    AddField(new FieldType
                    {
                        Name = nestedName,
                        Arguments = args,
                        ResolvedType = resolvedType,
                        Resolver = resolver,
                        Description = $"The {fieldDisplayName}/{nestedField.DisplayName()} nested field."
                    });
                }
            }

            Description = $"The structure of the {schemaName}.{fieldDisplayName} nested schema.";
        }
    }
}
