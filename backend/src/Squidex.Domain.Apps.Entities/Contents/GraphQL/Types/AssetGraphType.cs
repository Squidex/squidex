// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

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
                ResolvedType = AllTypes.NonNullGuid,
                Resolver = Resolve(x => x.Id.ToString()),
                Description = "The id of the asset."
            });

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = Resolve(x => x.Version),
                Description = "The version of the asset."
            });

            AddField(new FieldType
            {
                Name = "created",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = Resolve(x => x.Created),
                Description = "The date and time when the asset has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.CreatedBy.ToString()),
                Description = "The user that has created the asset."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = Resolve(x => x.LastModified),
                Description = "The date and time when the asset has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.LastModifiedBy.ToString()),
                Description = "The user that has updated the asset last."
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
                Resolver = model.ResolveAssetUrl(),
                Description = "The url to the asset."
            });

            AddField(new FieldType
            {
                Name = "thumbnailUrl",
                ResolvedType = AllTypes.String,
                Resolver = model.ResolveAssetThumbnailUrl(),
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
                Resolver = Resolve(x => x.FileName!.FileType()),
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
                ResolvedType = null,
                Resolver = Resolve(x => x.TagNames),
                Description = "The asset tags.",
                Type = AllTypes.NonNullTagsType
            });

            AddField(new FieldType
            {
                Name = "metadata",
                Arguments = AllTypes.PathArguments,
                ResolvedType = AllTypes.NoopJson,
                Resolver = ResolveMetadata(),
                Description = "The asset metadata.",
            });

            if (model.CanGenerateAssetSourceUrl)
            {
                AddField(new FieldType
                {
                    Name = "sourceUrl",
                    ResolvedType = AllTypes.NonNullString,
                    Resolver = model.ResolveAssetSourceUrl(),
                    Description = "The source url of the asset."
                });
            }

            Description = "An asset";
        }

        private static IFieldResolver ResolveMetadata()
        {
            return new FuncFieldResolver<IEnrichedAssetEntity, object?>(c =>
            {
                var path = c.Arguments.GetOrDefault(AllTypes.PathName);

                c.Source.Metadata.TryGetByPath(path as string, out var result);

                return result;
            });
        }

        private static IFieldResolver Resolve(Func<IEnrichedAssetEntity, object?> action)
        {
            return new FuncFieldResolver<IEnrichedAssetEntity, object?>(c => action(c.Source));
        }
    }
}
