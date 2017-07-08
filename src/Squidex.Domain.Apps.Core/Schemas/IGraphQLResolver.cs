// ==========================================================================
//  IGraphQLResolver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using GraphQL.Types;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public interface IGraphQLResolver
    {
        IGraphType GetSchemaListType(Schema schema);

        IGraphType GetSchemaListType(Guid schemaId);

        IGraphType GetAssetListType();
    }
}