// ==========================================================================
//  AssetGraphType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL.Types
{
    public sealed class AssetGraphType : ObjectGraphType<IAssetEntity>
    {
        public AssetGraphType(IGraphQLContext context)
        {
            Name = "AssetDto";

            AddField(new FieldType
            {
                Name = "id",
                Resolver = Resolver(x => x.Id.ToString()),
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Description = "The id of the asset."
            });

            AddField(new FieldType
            {
                Name = "version",
                Resolver = Resolver(x => x.Version),
                ResolvedType = new NonNullGraphType(new IntGraphType()),
                Description = "The version of the asset."
            });

            AddField(new FieldType
            {
                Name = "created",
                Resolver = Resolver(x => x.Created.ToDateTimeUtc()),
                ResolvedType = new NonNullGraphType(new DateGraphType()),
                Description = "The date and time when the asset has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                Resolver = Resolver(x => x.CreatedBy.ToString()),
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Description = "The user that has created the asset."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                Resolver = Resolver(x => x.LastModified.ToDateTimeUtc()),
                ResolvedType = new NonNullGraphType(new DateGraphType()),
                Description = "The date and time when the asset has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                Resolver = Resolver(x => x.LastModifiedBy.ToString()),
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Description = "The user that has updated the asset last."
            });

            AddField(new FieldType
            {
                Name = "mimeType",
                Resolver = Resolver(x => x.MimeType),
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Description = "The mime type."
            });

            AddField(new FieldType
            {
                Name = "url",
                Resolver = context.ResolveAssetUrl(),
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Description = "The url to the asset."
            });

            AddField(new FieldType
            {
                Name = "thumbnailUrl",
                Resolver = context.ResolveAssetThumbnailUrl(),
                ResolvedType = new StringGraphType(),
                Description = "The thumbnail url to the asset."
            });

            AddField(new FieldType
            {
                Name = "fileName",
                Resolver = Resolver(x => x.FileName),
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Description = "The file name."
            });

            AddField(new FieldType
            {
                Name = "fileType",
                Resolver = Resolver(x => x.FileName.FileType()),
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Description = "The file type."
            });

            AddField(new FieldType
            {
                Name = "fileSize",
                Resolver = Resolver(x => x.FileSize),
                ResolvedType = new NonNullGraphType(new IntGraphType()),
                Description = "The size of the file in bytes."
            });

            AddField(new FieldType
            {
                Name = "fileVersion",
                Resolver = Resolver(x => x.FileVersion),
                ResolvedType = new NonNullGraphType(new IntGraphType()),
                Description = "The version of the file."
            });

            AddField(new FieldType
            {
                Name = "isImage",
                Resolver = Resolver(x => x.IsImage),
                ResolvedType = new NonNullGraphType(new BooleanGraphType()),
                Description = "Determines of the created file is an image."
            });

            AddField(new FieldType
            {
                Name = "pixelWidth",
                Resolver = Resolver(x => x.PixelWidth),
                ResolvedType = new IntGraphType(),
                Description = "The width of the image in pixels if the asset is an image."
            });

            AddField(new FieldType
            {
                Name = "pixelHeight",
                Resolver = Resolver(x => x.PixelHeight),
                ResolvedType = new IntGraphType(),
                Description = "The height of the image in pixels if the asset is an image."
            });

            Description = "An asset";
        }

        private static IFieldResolver Resolver(Func<IAssetEntity, object> action)
        {
            return new FuncFieldResolver<IAssetEntity, object>(c => action(c.Source));
        }
    }
}
