// ==========================================================================
//  SchemaGraphType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public interface IGraphQLContext
    {
        IFieldPartitioning ResolvePartition(Partitioning key);

        IGraphType GetAssetType();

        IGraphType GetSchemaType(Guid schemaId);

        IFieldResolver ResolveAssetUrl();

        IFieldResolver ResolveAssetThumbnailUrl();

        IFieldResolver ResolveContentUrl(ISchemaEntity schemaEntity);

        (IGraphType ResolveType, IFieldResolver Resolver) GetGraphType(Field field);
    }
}
