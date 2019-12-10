// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Resolvers;
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

            foreach (var (nestedField, nestedName, _) in field.Fields.SafeFields())
            {
                var (resolveType, valueResolver) = model.GetGraphType(schema, nestedField, nestedName);

                if (resolveType != null && valueResolver != null)
                {
                    var resolver = ValueResolver(nestedField, valueResolver);

                    AddField(new FieldType
                    {
                        Name = nestedName,
                        Resolver = resolver,
                        ResolvedType = resolveType,
                        Description = $"The {fieldDisplayName}/{nestedField.DisplayName()} nested field."
                    });
                }
            }

            Description = $"The structure of the {schemaName}.{fieldDisplayName} nested schema.";
        }

        private static FuncFieldResolver<object?> ValueResolver(NestedField nestedField, ValueResolver resolver)
        {
            return new FuncFieldResolver<object?>(c =>
            {
                if (((JsonObject)c.Source).TryGetValue(nestedField.Name, out var value))
                {
                    return resolver(value, c);
                }
                else
                {
                    return null;
                }
            });
        }
    }
}
