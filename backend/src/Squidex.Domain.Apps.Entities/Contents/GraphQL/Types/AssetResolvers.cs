// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL;
using GraphQL.Resolvers;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class AssetResolvers
    {
        public static readonly IFieldResolver Url = Resolve((asset, _, context) =>
        {
            return context.UrlGenerator.AssetContent(asset.AppId, asset.Id.ToString());
        });

        public static readonly IFieldResolver SourceUrl = Resolve((asset, _, context) =>
        {
            return context.UrlGenerator.AssetSource(asset.AppId, asset.Id, asset.FileVersion);
        });

        public static readonly IFieldResolver ThumbnailUrl = Resolve((asset, _, context) =>
        {
            return context.UrlGenerator.AssetThumbnail(asset.AppId, asset.Id.ToString(), asset.Type);
        });

        public static readonly IFieldResolver FileHash = Resolve(x => x.FileHash);
        public static readonly IFieldResolver FileName = Resolve(x => x.FileName);
        public static readonly IFieldResolver FileSize = Resolve(x => x.FileSize);
        public static readonly IFieldResolver FileType = Resolve(x => x.FileName.FileType());
        public static readonly IFieldResolver FileVersion = Resolve(x => x.FileVersion);
        public static readonly IFieldResolver IsImage = Resolve(x => x.Type == AssetType.Image);
        public static readonly IFieldResolver IsProtected = Resolve(x => x.IsProtected);
        public static readonly IFieldResolver ListTotal = ResolveList(x => x.Total);
        public static readonly IFieldResolver ListItems = ResolveList(x => x);
        public static readonly IFieldResolver MetadataText = Resolve(x => x.MetadataText);
        public static readonly IFieldResolver MimeType = Resolve(x => x.MimeType);
        public static readonly IFieldResolver PixelHeight = Resolve(x => x.Metadata.GetPixelHeight());
        public static readonly IFieldResolver PixelWidth = Resolve(x => x.Metadata.GetPixelWidth());
        public static readonly IFieldResolver Slug = Resolve(x => x.Slug);
        public static readonly IFieldResolver Tags = Resolve(x => x.TagNames);
        public static readonly IFieldResolver Type = Resolve(x => x.Type);

        private static IFieldResolver Resolve<T>(Func<IEnrichedAssetEntity, IResolveFieldContext, GraphQLExecutionContext, T> action)
        {
            return new FuncFieldResolver<IEnrichedAssetEntity, object?>(c => action(c.Source, c, (GraphQLExecutionContext)c.UserContext));
        }

        private static IFieldResolver Resolve<T>(Func<IEnrichedAssetEntity, T> action)
        {
            return new FuncFieldResolver<IEnrichedAssetEntity, object?>(c => action(c.Source));
        }

        private static IFieldResolver ResolveList<T>(Func<IResultList<IEnrichedAssetEntity>, T> action)
        {
            return new FuncFieldResolver<IResultList<IEnrichedAssetEntity>, object?>(c => action(c.Source));
        }
    }
}
