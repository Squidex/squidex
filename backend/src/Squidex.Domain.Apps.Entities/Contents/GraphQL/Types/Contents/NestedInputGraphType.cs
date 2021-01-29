// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class NestedInputGraphType : InputObjectGraphType
    {
        public NestedInputGraphType(GraphQLModel model, FieldInfo fieldInfo)
        {
            Name = fieldInfo.NestedInputType;

            foreach (var nestedFieldInfo in fieldInfo.Fields)
            {
                var resolvedType = model.GetInputGraphType(nestedFieldInfo);

                if (resolvedType != null)
                {
                    AddField(new FieldType
                    {
                        Name = nestedFieldInfo.FieldName,
                        ResolvedType = resolvedType,
                        Resolver = null,
                        Description = $"The {fieldInfo.DisplayName}/{nestedFieldInfo.DisplayName} nested field."
                    }).WithSourceName(nestedFieldInfo);
                }
            }

            Description = $"The structure of the {fieldInfo.DisplayName} nested schema.";
        }
    }
}
