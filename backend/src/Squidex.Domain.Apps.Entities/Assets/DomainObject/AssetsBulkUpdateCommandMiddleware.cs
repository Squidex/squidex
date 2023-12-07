// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public sealed class AssetsBulkUpdateCommandMiddleware : ICommandMiddleware
{
    private readonly IContextProvider contextProvider;

    private sealed record BulkTask(
        BulkUpdateJob BulkJob,
        BulkUpdateAssets Bulk,
        int JobIndex,
        AssetCommand? Command)
    {
        public BulkUpdateResultItem? Result { get; private set; }

        public BulkTask SetResult(Exception? exception = null)
        {
            var id = Command?.AssetId ?? BulkJob.Id;

            Result = new BulkUpdateResultItem(id, JobIndex, exception);
            return this;
        }

        public static BulkTask Failed(BulkUpdateJob bulkJob, BulkUpdateAssets bulk, int jobIndex, Exception exception)
        {
            return new BulkTask(bulkJob, bulk, jobIndex, null).SetResult(exception);
        }
    }

    public AssetsBulkUpdateCommandMiddleware(IContextProvider contextProvider)
    {
        this.contextProvider = contextProvider;
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is not BulkUpdateAssets bulkUpdates)
        {
            await next(context, ct);
            return;
        }

        if (bulkUpdates.Jobs is not { Length: > 0 })
        {
            context.Complete(new BulkUpdateResult());
            return;
        }

        contextProvider.Context.Change(b => b
            .WithNoAssetEnrichment()
            .WithNoCleanup()
            .WithUnpublished(true)
            .WithNoTotal());

        var tasks = bulkUpdates.Jobs.Select((job, i) => CreateTask(job, bulkUpdates, i)).ToList();

        // Group the items by id, so that we do not run jobs in parallel on the same entity.
        var groupedTasks = tasks.GroupBy(x => x.BulkJob.Id).ToList();

        await Parallel.ForEachAsync(groupedTasks, ct, async (group, ct) =>
        {
            foreach (var task in group)
            {
                await ExecuteCommandAsync(context.CommandBus, task, ct);
            }
        });

        context.Complete(new BulkUpdateResult(tasks.Select(x => x.Result).NotNull()));
    }

    private static async Task ExecuteCommandAsync(ICommandBus commandBus, BulkTask task,
        CancellationToken ct)
    {
        if (task.Result != null || task.Command == null)
        {
            return;
        }

        try
        {
            await commandBus.PublishAsync(task.Command, ct);
            task.SetResult();
        }
        catch (Exception ex)
        {
            task.SetResult(ex);
        }
    }

    private BulkTask CreateTask(
        BulkUpdateJob bulkJob,
        BulkUpdateAssets bulk,
        int jobIndex)
    {
        try
        {
            switch (bulkJob.Type)
            {
                case BulkUpdateAssetType.Annotate:
                    return CreateTask<AnnotateAsset>(bulkJob, bulk, jobIndex,
                        PermissionIds.AppAssetsUpdate);

                case BulkUpdateAssetType.Move:
                    return CreateTask<MoveAsset>(bulkJob, bulk, jobIndex,
                        PermissionIds.AppAssetsUpdate);

                case BulkUpdateAssetType.Delete:
                    return CreateTask<DeleteAsset>(bulkJob, bulk, jobIndex,
                        PermissionIds.AppAssetsDelete);

                default:
                    return BulkTask.Failed(bulkJob, bulk, jobIndex, new NotSupportedException());
            }
        }
        catch (Exception ex)
        {
            return BulkTask.Failed(bulkJob, bulk, jobIndex, ex);
        }
    }

    private BulkTask CreateTask<T>(
        BulkUpdateJob bulkJob,
        BulkUpdateAssets bulk,
        int jobIndex,
        string permissionId) where T : AssetCommand, new()
    {
        if (!contextProvider.Context.Allows(permissionId))
        {
            return BulkTask.Failed(bulkJob, bulk, jobIndex, new DomainForbiddenException("Forbidden"));
        }

        var command = new T();

        SimpleMapper.Map(bulk, command);
        SimpleMapper.Map(bulkJob, command);

        command.ExpectedVersion = bulk.ExpectedVersion;
        command.AssetId = bulkJob.Id;

        return new BulkTask(bulkJob, bulk, jobIndex, command);
    }
}
