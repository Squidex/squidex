// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public sealed class AssetsBulkUpdateCommandMiddleware : ICommandMiddleware
{
    private readonly IContextProvider contextProvider;
    private readonly ILogger<AssetsBulkUpdateCommandMiddleware> log;

    private sealed record BulkTaskCommand(BulkTask Task, DomainId Id, ICommand Command,
        CancellationToken CancellationToken);

    private sealed record BulkTask(
        ICommandBus Bus,
        int JobIndex,
        BulkUpdateJob CommandJob,
        BulkUpdateAssets Command,
        ConcurrentBag<BulkUpdateResultItem> Results,
        CancellationToken Aborted);

    public AssetsBulkUpdateCommandMiddleware(IContextProvider contextProvider, ILogger<AssetsBulkUpdateCommandMiddleware> log)
    {
        this.contextProvider = contextProvider;

        this.log = log;
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is BulkUpdateAssets bulkUpdates)
        {
            if (bulkUpdates.Jobs?.Length > 0)
            {
                var executionOptions = new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
                };

                // Each job can create exactly one command.
                var createCommandsBlock = new TransformBlock<BulkTask, BulkTaskCommand?>(task =>
                {
                    try
                    {
                        return CreateCommand(task);
                    }
                    catch (OperationCanceledException ex)
                    {
                        // Dataflow swallows operation cancelled exception.
                        throw new AggregateException(ex);
                    }
                }, executionOptions);

                // Execute the commands in batches
                var executeCommandBlock = new ActionBlock<BulkTaskCommand?>(async command =>
                {
                    try
                    {
                        if (command != null)
                        {
                            await ExecuteCommandAsync(command);
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        // Dataflow swallows operation cancelled exception.
                        throw new AggregateException(ex);
                    }
                }, executionOptions);

                createCommandsBlock.BidirectionalLinkTo(executeCommandBlock);

                contextProvider.Context.Change(b => b
                    .WithoutAssetEnrichment()
                    .WithoutCleanup()
                    .WithUnpublished(true)
                    .WithoutTotal());

                var results = new ConcurrentBag<BulkUpdateResultItem>();

                for (var i = 0; i < bulkUpdates.Jobs.Length; i++)
                {
                    var task = new BulkTask(
                        context.CommandBus,
                        i,
                        bulkUpdates.Jobs[i],
                        bulkUpdates,
                        results,
                        ct);

                    if (!await createCommandsBlock.SendAsync(task, ct))
                    {
                        break;
                    }
                }

                createCommandsBlock.Complete();

                // Wait for all commands to be executed.
                await executeCommandBlock.Completion;

                context.Complete(new BulkUpdateResult(results));
            }
            else
            {
                context.Complete(new BulkUpdateResult());
            }
        }
        else
        {
            await next(context, ct);
        }
    }

    private async Task ExecuteCommandAsync(BulkTaskCommand bulkCommand)
    {
        var (task, id, command, ct) = bulkCommand;
        try
        {
            await task.Bus.PublishAsync(command, ct);

            task.Results.Add(new BulkUpdateResultItem(id, task.JobIndex));
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to execute asset bulk job with index {index} of type {type}.",
                task.JobIndex,
                task.CommandJob.Type);

            task.Results.Add(new BulkUpdateResultItem(id, task.JobIndex, ex));
        }
    }

    private BulkTaskCommand? CreateCommand(BulkTask task)
    {
        var id = task.CommandJob.Id;
        try
        {
            var command = CreateCommandCore(task);

            // Set the asset id here in case we have another way to resolve ids.
            command.AssetId = id;

            return new BulkTaskCommand(task, id, command, task.Aborted);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to execute asset bulk job with index {index} of type {type}.",
                task.JobIndex,
                task.CommandJob.Type);

            task.Results.Add(new BulkUpdateResultItem(id, task.JobIndex, ex));
            return null;
        }
    }

    private AssetCommand CreateCommandCore(BulkTask task)
    {
        var job = task.CommandJob;

        switch (job.Type)
        {
            case BulkUpdateAssetType.Annotate:
                {
                    var command = new AnnotateAsset();

                    EnrichAndCheckPermission(task, command, PermissionIds.AppAssetsUpdate);
                    return command;
                }

            case BulkUpdateAssetType.Move:
                {
                    var command = new MoveAsset();

                    EnrichAndCheckPermission(task, command, PermissionIds.AppAssetsUpdate);
                    return command;
                }

            case BulkUpdateAssetType.Delete:
                {
                    var command = new DeleteAsset();

                    EnrichAndCheckPermission(task, command, PermissionIds.AppAssetsDelete);
                    return command;
                }

            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    private void EnrichAndCheckPermission<T>(BulkTask task, T command, string permissionId) where T : AssetCommand
    {
        SimpleMapper.Map(task.Command, command);
        SimpleMapper.Map(task.CommandJob, command);

        if (!contextProvider.Context.Allows(permissionId))
        {
            throw new DomainForbiddenException("Forbidden");
        }

        command.ExpectedVersion = task.Command.ExpectedVersion;
    }
}
