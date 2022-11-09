// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class EmbeddableStringGraphType : ObjectGraphType<string>
{
    public EmbeddableStringGraphType(Builder builder, FieldInfo fieldInfo, StringFieldProperties properties)
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = fieldInfo.EmbeddableStringType;

        AddField(ContentFields.StringFieldText);
        AddField(ContentFields.StringFieldAssets);

        var referenceType = ResolveReferences(builder, fieldInfo, properties.SchemaIds);

        if (referenceType != null)
        {
            AddField(new FieldType
            {
                Name = "contents",
                ResolvedType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(referenceType))),
                Resolver = ContentFields.ResolveStringFieldContents,
                Description = FieldDescriptions.StringFieldReferences
            });
        }
    }

    private static IGraphType? ResolveReferences(Builder builder, FieldInfo fieldInfo, ReadonlyList<DomainId>? schemaIds)
    {
        IGraphType? contentType = null;

        if (schemaIds?.Count == 1)
        {
            contentType = builder.GetContentType(schemaIds[0]);
        }

        if (contentType == null)
        {
            var union = builder.GetReferenceUnion(fieldInfo, schemaIds);

            if (!union.HasType)
            {
                return default;
            }

            contentType = union;
        }

        return contentType;
    }
}
