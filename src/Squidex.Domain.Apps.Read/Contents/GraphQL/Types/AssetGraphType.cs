// ==========================================================================
//  AssetGraphType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Read.Assets;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL.Types
{
    public sealed class AssetGraphType : ObjectGraphType<IAssetEntity>
    {
        public AssetGraphType()
        {
            Name = "AssetDto";

            Field("id", x => x.Id.ToString())
                .Description("The id of the asset.");

            Field("version", x => x.Version)
                .Description("The version of the asset.");

            Field("created", x => x.Created.ToDateTimeUtc())
                .Description("The date and time when the asset has been created.");

            Field("createdBy", x => x.CreatedBy.ToString())
                .Description("The user that has created the asset.");

            Field("lastModified", x => x.LastModified.ToDateTimeUtc())
                .Description("The date and time when the asset has been modified last.");

            Field("lastModifiedBy", x => x.LastModifiedBy.ToString())
                .Description("The user that has updated the asset last.");

            Field("mimeType", x => x.MimeType)
                .Description("The mime type.");

            Field("fileName", x => x.FileName)
                .Description("The file name.");

            Field("fileSize", x => x.FileSize)
                .Description("The size of the file in bytes.");

            Field("fileVersion", x => x.FileVersion)
                .Description("The version of the file.");

            Field("isImage", x => x.IsImage)
                .Description("Determines of the created file is an image.");

            Field("pixelWidth", x => x.PixelWidth, true)
                .Description("The width of the image in pixels if the asset is an image.");

            Field("pixelHeight", x => x.PixelHeight, true)
                .Description("The height of the image in pixels if the asset is an image.");

            Description = "An asset";
        }
    }
}
