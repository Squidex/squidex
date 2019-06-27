// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public interface IGraphModel
    {
        bool CanGenerateAssetSourceUrl { get; }

        IFieldPartitioning ResolvePartition(Partitioning key);

        IFieldResolver ResolveAssetUrl();

        IFieldResolver ResolveAssetSourceUrl();

        IFieldResolver ResolveAssetThumbnailUrl();

        IFieldResolver ResolveContentUrl(ISchemaEntity schema);

        IGraphType GetAssetType();

        IGraphType GetContentType(Guid schemaId);

        IGraphType GetContentDataType(Guid schemaId);

        (IGraphType ResolveType, ValueResolver Resolver) GetGraphType(ISchemaEntity schema, IField field, string fieldName);
    }
}
