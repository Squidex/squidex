// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class ContentResultGraphType : ObjectGraphType<IResultList<IContentEntity>>
    {
        public ContentResultGraphType(ContentGraphType contentType, SchemaInfo schemaInfo)
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = schemaInfo.ResultType;

            AddField(new FieldType
            {
                Name = "total",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = ContentResolvers.ListTotal,
                Description = FieldDescriptions.ContentsTotal
            });

            AddField(new FieldType
            {
                Name = "items",
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = ContentResolvers.ListItems,
                Description = FieldDescriptions.ContentsItems
            });

            Description = $"List of {schemaInfo.DisplayName} items and total count.";
        }
    }
}
