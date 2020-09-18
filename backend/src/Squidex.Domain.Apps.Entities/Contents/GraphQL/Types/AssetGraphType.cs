// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Assets;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AssetGraphType : ObjectGraphType<IEnrichedAssetEntity>
    {
        public AssetGraphType(IGraphModel model)
        {
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
                ResolvedType = AllTypes.NonNullDate,
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
                Name = "lastModified",
                ResolvedType = AllTypes.NonNullDate,
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
                Name = "mimeType",
                ResolvedType = AllTypes.NonNullString,
                Resolver = AssetResolvers.MimeType,
                Description = "The mime type."
            });

            AddField(new FieldType
            {
                Name = "url",
                ResolvedType = AllTypes.NonNullString,
                Resolver = AssetResolvers.Url,
                Description = "The url to the asset."
            });

            AddField(new FieldType
            {
                Name = "thumbnailUrl",
                ResolvedType = AllTypes.String,
                Resolver = AssetResolvers.ThumbnailUrl,
                Description = "The thumbnail url to the asset."
            });

            AddField(new FieldType
            {
                Name = "fileName",
                ResolvedType = AllTypes.NonNullString,
                Resolver = AssetResolvers.FileName,
                Description = "The file name."
            });

            AddField(new FieldType
            {
                Name = "fileHash",
                ResolvedType = AllTypes.NonNullString,
                Resolver = AssetResolvers.FileHash,
                Description = "The hash of the file. Can be null for old files."
            });

            AddField(new FieldType
            {
                Name = "fileType",
                ResolvedType = AllTypes.NonNullString,
                Resolver = AssetResolvers.FileType,
                Description = "The file type."
            });

            AddField(new FieldType
            {
                Name = "fileSize",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = AssetResolvers.FileSize,
                Description = "The size of the file in bytes."
            });

            AddField(new FieldType
            {
                Name = "fileVersion",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = AssetResolvers.FileVersion,
                Description = "The version of the file."
            });

            AddField(new FieldType
            {
                Name = "slug",
                ResolvedType = AllTypes.NonNullString,
                Resolver = AssetResolvers.Slug,
                Description = "The file name as slug."
            });

            AddField(new FieldType
            {
                Name = "isProtected",
                ResolvedType = AllTypes.NonNullBoolean,
                Resolver = AssetResolvers.IsProtected,
                Description = "True, when the asset is not public."
            });

            AddField(new FieldType
            {
                Name = "isImage",
                ResolvedType = AllTypes.NonNullBoolean,
                Resolver = AssetResolvers.IsImage,
                Description = "Determines if the uploaded file is an image.",
                DeprecationReason = "Use 'type' field instead."
            });

            AddField(new FieldType
            {
                Name = "pixelWidth",
                ResolvedType = AllTypes.Int,
                Resolver = AssetResolvers.PixelWidth,
                Description = "The width of the image in pixels if the asset is an image.",
                DeprecationReason = "Use 'metadata' field instead."
            });

            AddField(new FieldType
            {
                Name = "pixelHeight",
                ResolvedType = AllTypes.Int,
                Resolver = AssetResolvers.PixelHeight,
                Description = "The height of the image in pixels if the asset is an image.",
                DeprecationReason = "Use 'metadata' field instead."
            });

            AddField(new FieldType
            {
                Name = "type",
                ResolvedType = AllTypes.NonNullAssetType,
                Resolver = AssetResolvers.Type,
                Description = "The type of the image."
            });

            AddField(new FieldType
            {
                Name = "metadataText",
                ResolvedType = AllTypes.NonNullString,
                Resolver = AssetResolvers.MetadataText,
                Description = "The text representation of the metadata."
            });

            AddField(new FieldType
            {
                Name = "tags",
                ResolvedType = null,
                Resolver = AssetResolvers.Tags,
                Description = "The asset tags.",
                Type = AllTypes.NonNullTagsType
            });

            AddField(new FieldType
            {
                Name = "metadata",
                Arguments = AssetActions.Metadata.Arguments,
                ResolvedType = AllTypes.NoopJson,
                Resolver = AssetActions.Metadata.Resolver,
                Description = "The asset metadata."
            });

            if (model.CanGenerateAssetSourceUrl)
            {
                AddField(new FieldType
                {
                    Name = "sourceUrl",
                    ResolvedType = AllTypes.NonNullString,
                    Resolver = AssetResolvers.SourceUrl,
                    Description = "The source url of the asset."
                });
            }

            Description = "An asset";
        }
    }
}
