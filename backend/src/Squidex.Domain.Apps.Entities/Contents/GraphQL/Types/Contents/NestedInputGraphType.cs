// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class NestedInputGraphType : InputObjectGraphType
{
    private readonly FieldMap fieldMap;

    public NestedInputGraphType(Builder builder, FieldInfo fieldInfo)
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = fieldInfo.NestedInputType;

        foreach (var nestedFieldInfo in fieldInfo.Fields)
        {
            var resolvedType = builder.GetInputGraphType(nestedFieldInfo);

            if (resolvedType == null)
            {
                continue;
            }

            AddField(new FieldTypeWithSourceName
            {
                Name = nestedFieldInfo.FieldName,
                ResolvedType = resolvedType,
                Resolver = null,
                Description = nestedFieldInfo.Field.RawProperties.Hints,
                SourceName = nestedFieldInfo.Field.Name,
            });
        }

        Description = $"The structure of the {fieldInfo.DisplayName} nested schema.";

        fieldMap = builder.FieldMap;
    }

    public override object ParseDictionary(IDictionary<string, object?> value)
    {
        return fieldMap.MapNested(this, value);
    }
}
