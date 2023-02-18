// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;

namespace TestSuite;

public static class Strategies
{
    public enum Deletion
    {
        SingleSoft,
        SinglePermanent,
        BulkSoft,
        BulkPermanent
    }

    public static Task DeleteAsync(this ISquidexClientManager clientManager, ContentBase content, Deletion strategy)
    {
        IContentsClient<MyContent, object> GetClient()
        {
            return clientManager.CreateContentsClient<MyContent, object>(content.SchemaName);
        }

        switch (strategy)
        {
            case Deletion.SingleSoft:
                return GetClient().DeleteAsync(content.Id);
            case Deletion.SinglePermanent:
                return GetClient().DeleteAsync(content.Id, new ContentDeleteOptions { Permanent = true });
            case Deletion.BulkSoft:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Delete,
                            Id = content.Id,
                            Permanent = false,
                        }
                    }
                });
            case Deletion.BulkPermanent:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Delete,
                            Id = content.Id,
                            Permanent = true,
                        }
                    }
                });
            default:
                return Task.CompletedTask;
        }
    }

    public enum Update
    {
        Normal,
        Upsert,
        UpsertBulk,
        Bulk,
        BulkWithSchema,
        BulkShared
    }

    public static Task UpdateAsync(this ISquidexClientManager clientManager, ContentBase content, object data, Update strategy)
    {
        IContentsClient<MyContent, object> GetClient()
        {
            return clientManager.CreateContentsClient<MyContent, object>(content.SchemaName);
        }

        switch (strategy)
        {
            case Update.Normal:
                return GetClient().UpdateAsync(content.Id, data);
            case Update.Upsert:
                return GetClient().UpsertAsync(content.Id, data);
            case Update.UpsertBulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Id = content.Id,
                            Data = data,
                            Type = BulkUpdateType.Upsert
                        }
                    }
                });
            case Update.Bulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Id = content.Id,
                            Data = data,
                            Type = BulkUpdateType.Update,
                        }
                    }
                });
            case Update.BulkWithSchema:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Id = content.Id,
                            Data = data,
                            Type = BulkUpdateType.Update,
                            Schema = content.SchemaName
                        }
                    }
                });
            case Update.BulkShared:
                return clientManager.CreateSharedContentsClient<MyContent, object>().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Id = content.Id,
                            Data = data,
                            Type = BulkUpdateType.Update,
                            Schema = content.SchemaName
                        }
                    }
                });
            default:
                return Task.CompletedTask;
        }
    }

    public enum Patch
    {
        Normal,
        Upsert,
        Bulk,
        BulkWithSchema,
        BulkShared,
        UpsertBulk
    }

    public static Task PatchAsync(this ISquidexClientManager clientManager, ContentBase content, object data, Patch strategy)
    {
        IContentsClient<MyContent, object> GetClient()
        {
            return clientManager.CreateContentsClient<MyContent, object>(content.SchemaName);
        }

        switch (strategy)
        {
            case Patch.Normal:
                return GetClient().PatchAsync(content.Id, data);
            case Patch.Upsert:
                return GetClient().UpsertAsync(content.Id, data, ContentUpsertOptions.AsPatch);
            case Patch.UpsertBulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Id = content.Id,
                            Data = data,
                            Patch = true,
                        }
                    }
                });
            case Patch.Bulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Id = content.Id,
                            Data = data,
                            Type = BulkUpdateType.Patch
                        }
                    }
                });
            case Patch.BulkWithSchema:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Id = content.Id,
                            Data = data,
                            Type = BulkUpdateType.Patch,
                            Schema = content.SchemaName
                        }
                    }
                });
            case Patch.BulkShared:
                return clientManager.CreateSharedContentsClient<MyContent, object>().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Id = content.Id,
                            Data = data,
                            Type = BulkUpdateType.Patch,
                            Schema = content.SchemaName
                        }
                    }
                });
            default:
                return Task.CompletedTask;
        }
    }

    private class MyContent : Content<object>
    {
    }
}
