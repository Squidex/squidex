// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;

#pragma warning disable MA0048 // File name must match type name

namespace TestSuite;

public static partial class ContentStrategies
{
    public static Task DeleteAsync(this IAssetsClient client, AssetDto asset, Deletion strategy)
    {
        switch (strategy)
        {
            case Deletion.SingleSoft:
                return client.DeleteAssetAsync(asset.Id);
            case Deletion.SinglePermanent:
                return client.DeleteAssetAsync(asset.Id, permanent: true);
            case Deletion.BulkSoft:
                return client.BulkUpdateAssetsAsync(new BulkUpdateAssetsDto
                {
                    Jobs =
                    [
                        new BulkUpdateAssetsJobDto
                        {
                            Type = BulkUpdateAssetType.Delete,
                            Id = asset.Id,
                            Permanent = false,
                        },
                    ]
                });
            case Deletion.BulkPermanent:
                return client.BulkUpdateAssetsAsync(new BulkUpdateAssetsDto
                {
                    Jobs =
                    [
                        new BulkUpdateAssetsJobDto
                        {
                            Type = BulkUpdateAssetType.Delete,
                            Id = asset.Id,
                            Permanent = true,
                        },
                    ]
                });
            default:
                return Task.CompletedTask;
        }
    }

    public enum Move
    {
        Single,
        Bulk
    }

    public static Task MoveAsync(this IAssetsClient client, AssetDto asset, AssetFolderDto folder, Move strategy)
    {
        switch (strategy)
        {
            case Move.Single:
                return client.PutAssetParentAsync(asset.Id, new MoveAssetDto
                {
                    ParentId = folder.Id
                });
            case Move.Bulk:
                return client.BulkUpdateAssetsAsync(new BulkUpdateAssetsDto
                {
                    Jobs =
                    [
                        new BulkUpdateAssetsJobDto
                        {
                            Type = BulkUpdateAssetType.Move,
                            Id = asset.Id,
                            ParentId = folder.Id
                        },
                    ]
                });
            default:
                return Task.CompletedTask;
        }
    }

    public enum Annotate
    {
        Single,
        Bulk
    }

    public static Task AnnotateAsync(this IAssetsClient client, AssetDto asset, AnnotateAssetDto request, Annotate strategy)
    {
        switch (strategy)
        {
            case Annotate.Single:
                return client.PutAssetAsync(asset.Id, request);
            case Annotate.Bulk:
                return client.BulkUpdateAssetsAsync(new BulkUpdateAssetsDto
                {
                    Jobs =
                    [
                        new BulkUpdateAssetsJobDto
                        {
                            Type = BulkUpdateAssetType.Annotate,
                            FileName = request.FileName,
                            Id = asset.Id,
                            IsProtected = request.IsProtected,
                            Metadata = request.Metadata,
                            Slug = request.Slug,
                            Tags = request.Tags
                        },
                    ]
                });
            default:
                return Task.CompletedTask;
        }
    }
}
