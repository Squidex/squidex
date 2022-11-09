// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class NestedInputGraphType : InputObjectGraphType
{
    public NestedInputGraphType(Builder builder, FieldInfo fieldInfo)
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = fieldInfo.NestedInputType;

        foreach (var nestedFieldInfo in fieldInfo.Fields)
        {
            var resolvedType = builder.GetInputGraphType(nestedFieldInfo);

            if (resolvedType != null)
            {
                AddField(new FieldType
                {
                    Name = nestedFieldInfo.FieldName,
                    ResolvedType = resolvedType,
                    Resolver = null,
                    Description = nestedFieldInfo.Field.RawProperties.Hints
                }).WithSourceName(nestedFieldInfo);
            }
        }

        Description = $"The structure of the {fieldInfo.DisplayName} nested schema.";
    }

    public override object ParseDictionary(IDictionary<string, object?> value)
    {
        var result = new JsonObject();

        foreach (var field in Fields)
        {
            if (value.TryGetValue(field.Name, out var fieldValue))
            {
                result[field.SourceName()] = JsonGraphType.ParseJson(fieldValue);
            }
        }

        return new JsonValue(result);
    }
}
