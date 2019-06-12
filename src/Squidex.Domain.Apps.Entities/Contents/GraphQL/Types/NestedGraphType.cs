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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class NestedGraphType : ObjectGraphType<JsonObject>
    {
        public NestedGraphType(IGraphModel model, ISchemaEntity schema, IArrayField field)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            var fieldName = field.DisplayName();

            Name = $"{schemaType}{fieldName}ChildDto";

            foreach (var (nestedField, nestedName, _) in field.Fields.SafeFields())
            {
                var fieldInfo = model.GetGraphType(schema, nestedField);

                if (fieldInfo.ResolveType != null)
                {
                    var resolver = ValueResolver(nestedField, fieldInfo);

                    AddField(new FieldType
                    {
                        Name = nestedName,
                        Resolver = resolver,
                        ResolvedType = fieldInfo.ResolveType,
                        Description = $"The {fieldName}/{nestedField.DisplayName()} nested field."
                    });
                }
            }

            Description = $"The structure of the {schemaName}.{fieldName} nested schema.";
        }

        private static FuncFieldResolver<object> ValueResolver(NestedField nestedField, (IGraphType ResolveType, ValueResolver Resolver) fieldInfo)
        {
            return new FuncFieldResolver<object>(c =>
            {
                if (((JsonObject)c.Source).TryGetValue(nestedField.Name, out var value))
                {
                    return fieldInfo.Resolver(value, c);
                }
                else
                {
                    return fieldInfo;
                }
            });
        }
    }
}
