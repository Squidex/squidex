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
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class NestedInputGraphType : InputObjectGraphType
    {
        public NestedInputGraphType(IGraphModel model, ISchemaEntity schema, IArrayField field)
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
                    AddField(new FieldType
                    {
                        Name = nestedField.Name.ToCamelCase(),
                        Resolver = null,
                        ResolvedType = fieldInfo.ResolveType,
                        Description = $"The {fieldName}/{nestedField.DisplayName()} nested field."
                    });
                }
            }

            Description = $"The structure of a {schemaName}.{fieldName} nested schema.";
        }
    }
}
