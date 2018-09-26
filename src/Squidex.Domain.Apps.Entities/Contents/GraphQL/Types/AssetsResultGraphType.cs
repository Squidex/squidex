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
    public sealed class AssetsResultGraphType : ObjectGraphType<IResultList<IAssetEntity>>
    {
        public AssetsResultGraphType(IGraphType assetType)
        {
            Name = "AssetResultDto";

            AddField(new FieldType
            {
                Name = "total",
                ResolvedType = AllTypes.Int,
                Resolver = Resolve(x => x.Total),
                Description = "The total count of assets."
            });

            AddField(new FieldType
            {
                Name = "items",
                Resolver = Resolve(x => x),
                ResolvedType = new ListGraphType(new NonNullGraphType(assetType)),
                Description = "The assets."
            });

            Description = "List of assets and total count of assets.";
        }

        private static IFieldResolver Resolve(Func<IResultList<IAssetEntity>, object> action)
        {
            return new FuncFieldResolver<IResultList<IAssetEntity>, object>(c => action(c.Source));
        }
    }
}
