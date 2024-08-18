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
    public enum Deletion
    {
        SingleSoft,
        SinglePermanent,
        BulkSoft,
        BulkPermanent
    }

    public static Task DeleteAsync(this ISquidexClient client, ContentBase content,
        Deletion strategy)
    {
        IContentsClient<MyContent, object> GetClient()
        {
            return client.Contents<MyContent, object>(content.SchemaName);
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
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Delete,
                            Id = content.Id,
                            Permanent = false,
                        },
                    ]
                });
            case Deletion.BulkPermanent:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Delete,
                            Id = content.Id,
                            Permanent = true,
                        },
                    ]
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

    public static Task UpdateAsync(this ISquidexClient client, ContentBase content, object data,
        Update strategy)
    {
        IContentsClient<MyContent, object> GetClient()
        {
            return client.Contents<MyContent, object>(content.SchemaName);
        }

        switch (strategy)
        {
            case Update.Normal:
                return GetClient().UpdateAsync(content.Id, data);
            case Update.Upsert:
                return GetClient().UpsertAsync(content.Id, data);
            case Update.Bulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Update,
                            Id = content.Id,
                            Data = data
                        },
                    ]
                });
            case Update.UpsertBulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Upsert,
                            Id = content.Id,
                            Data = data
                        },
                    ]
                });
            case Update.BulkWithSchema:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Update,
                            Id = content.Id,
                            Data = data,
                            Schema = content.SchemaName
                        },
                    ]
                });
            case Update.BulkShared:
                return GetSharedClient(client).BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Update,
                            Id = content.Id,
                            Data = data,
                            Schema = content.SchemaName
                        },
                    ]
                });
            default:
                return Task.CompletedTask;
        }
    }

    public enum Patch
    {
        Normal,
        Upsert,
        UpsertBulk,
        Bulk,
        BulkWithSchema,
        BulkShared
    }

    public static Task PatchAsync(this ISquidexClient client, ContentBase content, object data,
        Patch strategy)
    {
        IContentsClient<MyContent, object> GetClient()
        {
            return client.Contents<MyContent, object>(content.SchemaName);
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
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Upsert,
                            Id = content.Id,
                            Data = data,
                            Patch = true
                        },
                    ]
                });
            case Patch.Bulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Patch,
                            Id = content.Id,
                            Data = data,
                        },
                    ]
                });
            case Patch.BulkWithSchema:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Patch,
                            Id = content.Id,
                            Data = data,
                            Schema = content.SchemaName
                        },
                    ]
                });
            case Patch.BulkShared:
                return GetSharedClient(client).BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Patch,
                            Id = content.Id,
                            Data = data,
                            Schema = content.SchemaName
                        },
                    ]
                });
            default:
                return Task.CompletedTask;
        }
    }

    public enum EnrichDefaults
    {
        Normal,
        Upsert,
        Update,
        Bulk,
        BulkWithSchema,
        BulkShared,
        UpsertBulk,
        UpdateBulk
    }

    public static Task EnrichDefaultsAsync(this ISquidexClient client, ContentBase content, object data,
        EnrichDefaults strategy, bool requiredFields)
    {
        IContentsClient<MyContent, object> GetClient()
        {
            return client.Contents<MyContent, object>(content.SchemaName);
        }

        switch (strategy)
        {
            case EnrichDefaults.Normal:
                var createOptions = new ContentEnrichDefaultsOptions
                {
                    EnrichRequiredFields = requiredFields
                };

                return GetClient().EnrichDefaultsAsync(content.Id, createOptions);
            case EnrichDefaults.Update:
                var updateOptions = new ContentUpdateOptions
                {
                    EnrichDefaults = true
                };

                return GetClient().UpdateAsync(content.Id, data, updateOptions);
            case EnrichDefaults.Upsert:
                var upsertOptions = new ContentUpsertOptions
                {
                    EnrichDefaults = true,
                    EnrichRequiredFields = requiredFields
                };

                return GetClient().UpsertAsync(content.Id, data, upsertOptions);
            case EnrichDefaults.Bulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.EnrichDefaults,
                            Id = content.Id
                        },
                    ],
                    EnrichRequiredFields = requiredFields
                });
            case EnrichDefaults.UpdateBulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Update,
                            Id = content.Id,
                            Data = data,
                            EnrichDefaults = true,
                        },
                    ],
                    EnrichRequiredFields = requiredFields
                });
            case EnrichDefaults.UpsertBulk:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Upsert,
                            Id = content.Id,
                            Data = data,
                            EnrichDefaults = true,
                        },
                    ],
                    EnrichRequiredFields = requiredFields
                });
            case EnrichDefaults.BulkWithSchema:
                return GetClient().BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.EnrichDefaults,
                            Id = content.Id,
                            Schema = content.SchemaName
                        },
                    ],
                    EnrichRequiredFields = requiredFields
                });
            case EnrichDefaults.BulkShared:
                return GetSharedClient(client).BulkUpdateAsync(new BulkUpdate
                {
                    Jobs =
                    [
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.EnrichDefaults,
                            Id = content.Id,
                            Schema = content.SchemaName
                        },
                    ],
                    EnrichRequiredFields = requiredFields
                });
            default:
                return Task.CompletedTask;
        }
    }

    private static IContentsSharedClient<MyContent, object> GetSharedClient(ISquidexClient client)
    {
        return client.SharedContents<MyContent, object>();
    }

    private sealed class MyContent : Content<object>
    {
    }
}
