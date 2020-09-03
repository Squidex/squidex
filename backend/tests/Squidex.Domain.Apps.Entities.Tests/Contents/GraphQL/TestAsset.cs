﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public static class TestAsset
    {
        public const string AllFields = @"
            id
            version
            created
            createdBy
            lastModified
            lastModifiedBy
            url
            thumbnailUrl
            sourceUrl
            mimeType
            fileName
            fileHash
            fileSize
            fileVersion
            isImage
            isProtected
            pixelWidth
            pixelHeight
            tags
            type
            metadataText
            metadataPixelWidth: metadata(path: ""pixelWidth"")
            metadataUnknown: metadata(path: ""unknown"")
            metadata
            slug";

        public static IEnrichedAssetEntity Create(Guid id)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var asset = new AssetEntity
            {
                Id = id,
                Version = 1,
                Created = now,
                CreatedBy = new RefToken(RefTokenType.Subject, "user1"),
                LastModified = now,
                LastModifiedBy = new RefToken(RefTokenType.Subject, "user2"),
                FileName = "MyFile.png",
                Slug = "myfile.png",
                FileSize = 1024,
                FileHash = "ABC123",
                FileVersion = 123,
                MimeType = "image/png",
                Type = AssetType.Image,
                MetadataText = "metadata-text",
                Metadata =
                    new AssetMetadata()
                        .SetPixelWidth(800)
                        .SetPixelHeight(600),
                TagNames = new[]
                {
                    "tag1",
                    "tag2"
                }.ToHashSet()
            };

            return asset;
        }

        public static object Response(IEnrichedAssetEntity asset)
        {
            return new
            {
                id = asset.Id,
                version = asset.Version,
                created = asset.Created,
                createdBy = asset.CreatedBy.ToString(),
                lastModified = asset.LastModified,
                lastModifiedBy = asset.LastModifiedBy.ToString(),
                url = $"assets/{asset.Id}",
                thumbnailUrl = $"assets/{asset.Id}?width=100",
                sourceUrl = $"assets/source/{asset.Id}",
                mimeType = asset.MimeType,
                fileName = asset.FileName,
                fileHash = asset.FileHash,
                fileSize = asset.FileSize,
                fileVersion = asset.FileVersion,
                isImage = true,
                isProtected = asset.IsProtected,
                pixelWidth = asset.Metadata.GetPixelWidth(),
                pixelHeight = asset.Metadata.GetPixelHeight(),
                tags = asset.TagNames,
                type = "IMAGE",
                metadataText = asset.MetadataText,
                metadataPixelWidth = 800,
                metadataUnknown = (string?)null,
                metadata = new
                {
                    pixelWidth = asset.Metadata.GetPixelWidth(),
                    pixelHeight = asset.Metadata.GetPixelHeight()
                },
                slug = asset.Slug,
            };
        }
    }
}
