// ==========================================================================
//  SchemaGraphType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Read.GraphQl
{
    public interface IGraphQLContext
    {
        IGraphType GetSchemaListType(Schema schema);

        IFieldPartitioning ResolvePartition(Partitioning key);

        (IGraphType ResolveType, IFieldResolver Resolver) GetGraphType(Field field);
    }
}
