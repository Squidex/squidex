// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AssetResultGraphType : ObjectGraphType<IResultList<IAssetEntity>>
    {
        public AssetResultGraphType(IGraphType assetType)
        {
            Name = $"AssetResultDto";

            AddField(new FieldType
            {
                Name = "total",
                Resolver = Resolver(x => x.Total),
                ResolvedType = new NonNullGraphType(new IntGraphType()),
                Description = $"The total number of asset."
            });

            AddField(new FieldType
            {
                Name = "items",
                Resolver = Resolver(x => x),
                ResolvedType = new ListGraphType(new NonNullGraphType(assetType)),
                Description = $"The assets."
            });
        }

        private static IFieldResolver Resolver(Func<IResultList<IAssetEntity>, object> action)
        {
            return new FuncFieldResolver<IResultList<IAssetEntity>, object>(c => action(c.Source));
        }
    }
}
