// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class NestedGraphType : ObjectGraphType<JObject>
    {
        public NestedGraphType(IGraphModel model, ISchemaEntity schema, IArrayField field)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            var fieldType = field.TypeName();
            var fieldName = field.DisplayName();

            Name = $"{schemaType}{fieldName}ChildDto";

            foreach (var nestedField in field.Fields.Where(x => !x.IsHidden))
            {
                var fieldInfo = model.GetGraphType(schema, nestedField);

                if (fieldInfo.ResolveType != null)
                {
                    var resolver = new FuncFieldResolver<object>(c =>
                    {
                        if (((JObject)c.Source).TryGetValue(nestedField.Name, out var value))
                        {
                            return fieldInfo.Resolver(value, c);
                        }
                        else
                        {
                            return fieldInfo;
                        }
                    });

                    AddField(new FieldType
                    {
                        Name = nestedField.Name.ToCamelCase(),
                        Resolver = resolver,
                        ResolvedType = fieldInfo.ResolveType,
                        Description = $"The {fieldName}/{nestedField.DisplayName()} nested field."
                    });
                }
            }

            Description = $"The structure of the {schemaName}.{fieldName} nested schema.";
        }
    }
}
