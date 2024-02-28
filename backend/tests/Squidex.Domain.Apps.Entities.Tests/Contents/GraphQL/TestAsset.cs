// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public static class TestAsset
{
    public const string AllFields = @"
            id
            version
            created
            createdBy
            createdByUser {
              id
              email
              displayName
            }
            editToken
            lastModified
            lastModifiedBy
            lastModifiedByUser {
              id
              email
              displayName
            }
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
            parentId
            tags
            type
            metadataText
            metadataPixelWidth: metadata(path: ""pixelWidth"")
            metadataUnknown: metadata(path: ""unknown"")
            metadata
            slug";

    public static EnrichedAsset Create(DomainId id)
    {
        var now = SystemClock.Instance.GetCurrentInstant();

        var asset = new EnrichedAsset
        {
            Id = id,
            AppId = TestApp.DefaultId,
            Version = 1,
            Created = now,
            CreatedBy = RefToken.User("user1"),
            EditToken = $"token_{id}",
            LastModified = now,
            LastModifiedBy = RefToken.Client("client1"),
            FileName = "MyFile.png",
            Slug = "myfile.png",
            FileSize = 1024,
            FileHash = "ABC123",
            FileVersion = 123,
            ParentId = id,
            MimeType = "image/png",
            Type = AssetType.Image,
            MetadataText = "metadata-text",
            Metadata = new AssetMetadata
            {
                [KnownMetadataKeys.PixelWidth] = 800,
                [KnownMetadataKeys.PixelHeight] = 600,
            },
            TagNames =
            [
                "tag1",
                "tag2",
            ]
        };

        return asset;
    }

    public static object Response(EnrichedAsset asset)
    {
        return new
        {
            id = asset.Id,
            version = asset.Version,
            created = asset.Created,
            createdBy = asset.CreatedBy.ToString(),
            createdByUser = new
            {
                id = asset.CreatedBy.Identifier,
                email = $"{asset.CreatedBy.Identifier}@email.com",
                displayName = $"{asset.CreatedBy.Identifier}name"
            },
            editToken = $"token_{asset.Id}",
            lastModified = asset.LastModified,
            lastModifiedBy = asset.LastModifiedBy.ToString(),
            lastModifiedByUser = new
            {
                id = asset.LastModifiedBy.Identifier,
                email = $"{asset.LastModifiedBy}",
                displayName = asset.LastModifiedBy.Identifier
            },
            url = $"assets/{asset.AppId.Name}/{asset.Id}",
            thumbnailUrl = $"assets/{asset.AppId.Name}/{asset.Id}?width=100",
            sourceUrl = $"assets/source/{asset.Id}",
            mimeType = asset.MimeType,
            fileName = asset.FileName,
            fileHash = asset.FileHash,
            fileSize = asset.FileSize,
            fileVersion = asset.FileVersion,
            isImage = true,
            isProtected = asset.IsProtected,
            pixelWidth = asset.Metadata.GetInt32(KnownMetadataKeys.PixelWidth),
            pixelHeight = asset.Metadata.GetInt32(KnownMetadataKeys.PixelHeight),
            parentId = asset.Id,
            tags = asset.TagNames,
            type = "IMAGE",
            metadataText = asset.MetadataText,
            metadataPixelWidth = 800,
            metadataUnknown = (string?)null,
            metadata = new
            {
                pixelWidth = asset.Metadata.GetInt32(KnownMetadataKeys.PixelWidth),
                pixelHeight = asset.Metadata.GetInt32(KnownMetadataKeys.PixelHeight)
            },
            slug = asset.Slug
        };
    }
}
