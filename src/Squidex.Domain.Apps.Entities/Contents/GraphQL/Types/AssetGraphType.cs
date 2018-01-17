// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AssetGraphType : ObjectGraphType<IAssetEntity>
    {
        public AssetGraphType(IGraphModel model)
        {
            Name = "AssetDto";

            AddField(new FieldType
            {
                Name = "id",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = Resolve(x => x.Id.ToString()),
                Description = "The id of the asset."
            });

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = new NonNullGraphType(new IntGraphType()),
                Resolver = Resolve(x => x.Version),
                Description = "The version of the asset."
            });

            AddField(new FieldType
            {
                Name = "created",
                ResolvedType = new NonNullGraphType(new DateGraphType()),
                Resolver = Resolve(x => x.Created.ToDateTimeUtc()),
                Description = "The date and time when the asset has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = Resolve(x => x.CreatedBy.ToString()),
                Description = "The user that has created the asset."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                ResolvedType = new NonNullGraphType(new DateGraphType()),
                Resolver = Resolve(x => x.LastModified.ToDateTimeUtc()),
                Description = "The date and time when the asset has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = Resolve(x => x.LastModifiedBy.ToString()),
                Description = "The user that has updated the asset last."
            });

            AddField(new FieldType
            {
                Name = "mimeType",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = Resolve(x => x.MimeType),
                Description = "The mime type."
            });

            AddField(new FieldType
            {
                Name = "url",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = model.ResolveAssetUrl(),
                Description = "The url to the asset."
            });

            AddField(new FieldType
            {
                Name = "thumbnailUrl",
                ResolvedType = new StringGraphType(),
                Resolver = model.ResolveAssetThumbnailUrl(),
                Description = "The thumbnail url to the asset."
            });

            AddField(new FieldType
            {
                Name = "fileName",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = Resolve(x => x.FileName),
                Description = "The file name."
            });

            AddField(new FieldType
            {
                Name = "fileType",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = Resolve(x => x.FileName.FileType()),
                Description = "The file type."
            });

            AddField(new FieldType
            {
                Name = "fileSize",
                ResolvedType = new NonNullGraphType(new IntGraphType()),
                Resolver = Resolve(x => x.FileSize),
                Description = "The size of the file in bytes."
            });

            AddField(new FieldType
            {
                Name = "fileVersion",
                ResolvedType = new NonNullGraphType(new IntGraphType()),
                Resolver = Resolve(x => x.FileVersion),
                Description = "The version of the file."
            });

            AddField(new FieldType
            {
                Name = "isImage",
                ResolvedType = new NonNullGraphType(new BooleanGraphType()),
                Resolver = Resolve(x => x.IsImage),
                Description = "Determines of the created file is an image."
            });

            AddField(new FieldType
            {
                Name = "pixelWidth",
                ResolvedType = new IntGraphType(),
                Resolver = Resolve(x => x.PixelWidth),
                Description = "The width of the image in pixels if the asset is an image."
            });

            AddField(new FieldType
            {
                Name = "pixelHeight",
                ResolvedType = new IntGraphType(),
                Resolver = Resolve(x => x.PixelHeight),
                Description = "The height of the image in pixels if the asset is an image."
            });

            if (model.CanGenerateAssetSourceUrl)
            {
                AddField(new FieldType
                {
                    Name = "sourceUrl",
                    ResolvedType = new StringGraphType(),
                    Resolver = model.ResolveAssetSourceUrl(),
                    Description = "The source url of the asset."
                });
            }

            Description = "An asset";
        }

        private static IFieldResolver Resolve(Func<IAssetEntity, object> action)
        {
            return new FuncFieldResolver<IAssetEntity, object>(c => action(c.Source));
        }
    }
}
