// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Assets
{
    internal sealed class AssetGraphType : ObjectGraphType<IEnrichedAssetEntity>
    {
        public AssetGraphType(bool canGenerateSourceUrl)
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = "Asset";

            AddField(new FieldType
            {
                Name = "id",
                ResolvedType = AllTypes.NonNullString,
                Resolver = EntityResolvers.Id,
                Description = "The id of the asset."
            });

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = EntityResolvers.Version,
                Description = "The version of the asset."
            });

            AddField(new FieldType
            {
                Name = "created",
                ResolvedType = AllTypes.NonNullDateTime,
                Resolver = EntityResolvers.Created,
                Description = "The date and time when the asset has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = EntityResolvers.CreatedBy,
                Description = "The user that has created the asset."
            });

            AddField(new FieldType
            {
                Name = "createdByUser",
                ResolvedType = UserGraphType.NonNull,
                Resolver = EntityResolvers.CreatedByUser,
                Description = "The full info of the user that has created the asset."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                ResolvedType = AllTypes.NonNullDateTime,
                Resolver = EntityResolvers.LastModified,
                Description = "The date and time when the asset has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = EntityResolvers.LastModifiedBy,
                Description = "The user that has updated the asset last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedByUser",
                ResolvedType = UserGraphType.NonNull,
                Resolver = EntityResolvers.LastModifiedByUser,
                Description = "The full info of the user that has created the asset."
            });

            AddField(new FieldType
            {
                Name = "mimeType",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.MimeType),
                Description = "The mime type."
            });

            AddField(new FieldType
            {
                Name = "url",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Url,
                Description = "The url to the asset."
            });

            AddField(new FieldType
            {
                Name = "thumbnailUrl",
                ResolvedType = AllTypes.String,
                Resolver = ThumbnailUrl,
                Description = "The thumbnail url to the asset."
            });

            AddField(new FieldType
            {
                Name = "fileName",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.FileName),
                Description = "The file name."
            });

            AddField(new FieldType
            {
                Name = "fileHash",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.FileHash),
                Description = "The hash of the file. Can be null for old files."
            });

            AddField(new FieldType
            {
                Name = "fileType",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.FileName.FileType()),
                Description = "The file type."
            });

            AddField(new FieldType
            {
                Name = "fileSize",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = Resolve(x => x.FileSize),
                Description = "The size of the file in bytes."
            });

            AddField(new FieldType
            {
                Name = "fileVersion",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = Resolve(x => x.FileVersion),
                Description = "The version of the file."
            });

            AddField(new FieldType
            {
                Name = "slug",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.Slug),
                Description = "The file name as slug."
            });

            AddField(new FieldType
            {
                Name = "isProtected",
                ResolvedType = AllTypes.NonNullBoolean,
                Resolver = Resolve(x => x.IsProtected),
                Description = "True, when the asset is not public."
            });

            AddField(new FieldType
            {
                Name = "isImage",
                ResolvedType = AllTypes.NonNullBoolean,
                Resolver = Resolve(x => x.Type == AssetType.Image),
                Description = "Determines if the uploaded file is an image.",
                DeprecationReason = "Use 'type' field instead."
            });

            AddField(new FieldType
            {
                Name = "pixelWidth",
                ResolvedType = AllTypes.Int,
                Resolver = Resolve(x => x.Metadata.GetPixelWidth()),
                Description = "The width of the image in pixels if the asset is an image.",
                DeprecationReason = "Use 'metadata' field instead."
            });

            AddField(new FieldType
            {
                Name = "pixelHeight",
                ResolvedType = AllTypes.Int,
                Resolver = Resolve(x => x.Metadata.GetPixelHeight()),
                Description = "The height of the image in pixels if the asset is an image.",
                DeprecationReason = "Use 'metadata' field instead."
            });

            AddField(new FieldType
            {
                Name = "type",
                ResolvedType = AllTypes.NonNullAssetType,
                Resolver = Resolve(x => x.Type),
                Description = "The type of the image."
            });

            AddField(new FieldType
            {
                Name = "metadataText",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.MetadataText),
                Description = "The text representation of the metadata."
            });

            AddField(new FieldType
            {
                Name = "tags",
                ResolvedType = AllTypes.NonNullStrings,
                Resolver = Resolve(x => x.TagNames),
                Description = "The asset tags."
            });

            AddField(new FieldType
            {
                Name = "metadata",
                Arguments = AssetActions.Metadata.Arguments,
                ResolvedType = AllTypes.JsonNoop,
                Resolver = AssetActions.Metadata.Resolver,
                Description = "The asset metadata."
            });

            if (canGenerateSourceUrl)
            {
                AddField(new FieldType
                {
                    Name = "sourceUrl",
                    ResolvedType = AllTypes.NonNullString,
                    Resolver = SourceUrl,
                    Description = "The source url of the asset."
                });
            }

            Description = "An asset";
        }

        private static readonly IFieldResolver Url = Resolve((asset, _, context) =>
        {
            return context.UrlGenerator.AssetContent(asset.AppId, asset.Id.ToString());
        });

        private static readonly IFieldResolver SourceUrl = Resolve((asset, _, context) =>
        {
            return context.UrlGenerator.AssetSource(asset.AppId, asset.Id, asset.FileVersion);
        });

        private static readonly IFieldResolver ThumbnailUrl = Resolve((asset, _, context) =>
        {
            return context.UrlGenerator.AssetThumbnail(asset.AppId, asset.Id.ToString(), asset.Type);
        });

        private static IFieldResolver Resolve<T>(Func<IEnrichedAssetEntity, IResolveFieldContext, GraphQLExecutionContext, T> resolver)
        {
            return Resolvers.Sync(resolver);
        }

        private static IFieldResolver Resolve<T>(Func<IEnrichedAssetEntity, T> resolver)
        {
            return Resolvers.Sync(resolver);
        }
    }
}
