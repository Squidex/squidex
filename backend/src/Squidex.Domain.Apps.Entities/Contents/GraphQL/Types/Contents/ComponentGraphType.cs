// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class ComponentGraphType : ObjectGraphType<JsonObject>
{
    public ComponentGraphType(SchemaInfo schemaInfo)
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = schemaInfo.ComponentType;

        IsTypeOf = CheckType(schemaInfo.Schema.Id.ToString());
    }

    public void Initialize(Builder builder, SchemaInfo schemaInfo)
    {
        Description = $"The structure of the {schemaInfo.DisplayName} component schema.";

        AddField(ContentFields.SchemaId);

        foreach (var fieldInfo in schemaInfo.Fields)
        {
            if (fieldInfo.Field.IsComponentLike())
            {
                AddField(new FieldType
                {
                    Name = fieldInfo.FieldNameDynamic,
                    Arguments = ContentActions.Json.Arguments,
                    ResolvedType = Scalars.Json,
                    Resolver = FieldVisitor.JsonPath,
                    Description = fieldInfo.Field.RawProperties.Hints
                }).WithSourceName(fieldInfo);
            }

            var (resolvedType, resolver, args) = builder.GetGraphType(fieldInfo);

            if (resolvedType != null && resolver != null)
            {
                AddField(new FieldType
                {
                    Name = fieldInfo.FieldName,
                    Arguments = args,
                    ResolvedType = resolvedType,
                    Resolver = resolver,
                    Description = fieldInfo.Field.RawProperties.Hints
                }).WithSourceName(fieldInfo);
            }
        }

        AddResolvedInterface(builder.ComponentInterface);
    }

    private static Func<object, bool> CheckType(string schemaId)
    {
        return value =>
        {
            if (value is not JsonObject json)
            {
                return false;
            }

            return Component.IsValid(json, out var discriminator) && discriminator == schemaId;
        };
    }
}
