// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
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
                Description = $"The total number of {schemaInfo.DisplayName} items."
            });

            AddField(new FieldType
            {
                Name = "items",
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = ContentResolvers.ListItems,
                Description = $"The {schemaInfo.DisplayName} items."
            });

            Description = $"List of {schemaInfo.DisplayName} items and total count.";
        }
    }
}
