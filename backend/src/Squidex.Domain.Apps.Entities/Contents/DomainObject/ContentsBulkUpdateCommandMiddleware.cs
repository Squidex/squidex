// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public sealed class ContentsBulkUpdateCommandMiddleware : ICommandMiddleware
{
    private readonly IContentQueryService contentQuery;
    private readonly IContextProvider contextProvider;
    private readonly ILogger<ContentsBulkUpdateCommandMiddleware> log;

    private sealed record BulkTaskCommand(BulkTask Task, DomainId Id, ICommand Command,
        CancellationToken CancellationToken);

    private sealed record BulkTask(
        ICommandBus Bus,
        NamedId<DomainId> SchemaId,
        int JobIndex,
        BulkUpdateJob CommandJob,
        BulkUpdateContents Command,
        ConcurrentBag<BulkUpdateResultItem> Results,
        CancellationToken Aborted);

    public ContentsBulkUpdateCommandMiddleware(
        IContentQueryService contentQuery,
        IContextProvider contextProvider,
        ILogger<ContentsBulkUpdateCommandMiddleware> log)
    {
        this.contentQuery = contentQuery;
        this.contextProvider = contextProvider;

        this.log = log;
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is BulkUpdateContents bulkUpdates)
        {
            if (bulkUpdates.Jobs?.Length > 0)
            {
                var executionOptions = new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
                };

                // Each job can create one or more commands.
                var createCommandsBlock = new TransformManyBlock<BulkTask, BulkTaskCommand>(async task =>
                {
                    try
                    {
                        return await CreateCommandsAsync(task);
                    }
                    catch (OperationCanceledException ex)
                    {
                        // Dataflow swallows operation cancelled exception.
                        throw new AggregateException(ex);
                    }
                }, executionOptions);

                // Execute the commands in batches.
                var executeCommandBlock = new ActionBlock<BulkTaskCommand>(async command =>
                {
                    try
                    {
                        await ExecuteCommandAsync(command);
                    }
                    catch (OperationCanceledException ex)
                    {
                        // Dataflow swallows operation cancelled exception.
                        throw new AggregateException(ex);
                    }
                }, executionOptions);

                createCommandsBlock.BidirectionalLinkTo(executeCommandBlock);

                contextProvider.Context.Change(b => b
                    .WithoutContentEnrichment()
                    .WithoutCleanup()
                    .WithUnpublished(true)
                    .WithoutTotal());

                var results = new ConcurrentBag<BulkUpdateResultItem>();

                for (var i = 0; i < bulkUpdates.Jobs.Length; i++)
                {
                    var task = new BulkTask(
                        context.CommandBus,
                        bulkUpdates.SchemaId,
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
            log.LogError(ex, "Failed to execute content bulk job with index {index} of type {type}.",
                task.JobIndex,
                task.CommandJob.Type);

            task.Results.Add(new BulkUpdateResultItem(id, task.JobIndex, ex));
        }
    }

    private async Task<IEnumerable<BulkTaskCommand>> CreateCommandsAsync(BulkTask task)
    {
        // The task parallel pipeline does not allow async-enumerable.
        var commands = new List<BulkTaskCommand>();
        try
        {
            // Check whether another schema is defined for the current job and override the schema id if necessary.
            var overridenSchema = task.CommandJob.Schema;

            if (!string.IsNullOrWhiteSpace(overridenSchema))
            {
                var schema = await contentQuery.GetSchemaOrThrowAsync(contextProvider.Context, overridenSchema, task.Aborted);

                // Task is immutable, so we have to create a copy.
                task = task with { SchemaId = schema.NamedId() };
            }

            // The bulk command can be invoke in a schema controller or without a schema controller, therefore the name might be null.
            if (task.SchemaId == null || task.SchemaId.Id == default)
            {
                throw new DomainObjectNotFoundException("undefined");
            }

            var resolvedIds = await FindIdAsync(task, task.SchemaId.Name);

            if (resolvedIds.Length == 0)
            {
                throw new DomainObjectNotFoundException("undefined");
            }

            foreach (var id in resolvedIds)
            {
                try
                {
                    var command = CreateCommand(task);

                    command.ContentId = id;
                    commands.Add(new BulkTaskCommand(task, id, command, task.Aborted));
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to create content bulk job with index {index} of type {type}.",
                        task.JobIndex,
                        task.CommandJob.Type);

                    task.Results.Add(new BulkUpdateResultItem(id, task.JobIndex, ex));
                }
            }
        }
        catch (Exception ex)
        {
            task.Results.Add(new BulkUpdateResultItem(task.CommandJob.Id, task.JobIndex, ex));
        }

        return commands;
    }

    private ContentCommand CreateCommand(BulkTask task)
    {
        var job = task.CommandJob;

        switch (job.Type)
        {
            case BulkUpdateContentType.Create:
                {
                    var command = new CreateContent();

                    EnrichAndCheckPermission(task, command, PermissionIds.AppContentsCreate);
                    return command;
                }

            case BulkUpdateContentType.Update:
                {
                    var command = new UpdateContent();

                    EnrichAndCheckPermission(task, command, PermissionIds.AppContentsUpdateOwn);
                    return command;
                }

            case BulkUpdateContentType.Upsert:
                {
                    var command = new UpsertContent();

                    EnrichAndCheckPermission(task, command, PermissionIds.AppContentsUpsert);
                    return command;
                }

            case BulkUpdateContentType.Patch:
                {
                    var command = new PatchContent();

                    EnrichAndCheckPermission(task, command, PermissionIds.AppContentsUpdateOwn);
                    return command;
                }

            case BulkUpdateContentType.Validate:
                {
                    var command = new ValidateContent();

                    EnrichAndCheckPermission(task, command, PermissionIds.AppContentsReadOwn);
                    return command;
                }

            case BulkUpdateContentType.ChangeStatus:
                {
                    var command = new ChangeContentStatus { Status = job.Status ?? Status.Draft };

                    EnrichAndCheckPermission(task, command, PermissionIds.AppContentsChangeStatusOwn);
                    return command;
                }

            case BulkUpdateContentType.Delete:
                {
                    var command = new DeleteContent();

                    EnrichAndCheckPermission(task, command, PermissionIds.AppContentsDeleteOwn);
                    return command;
                }

            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    private void EnrichAndCheckPermission<T>(BulkTask task, T command, string permissionId) where T : ContentCommand
    {
        SimpleMapper.Map(task.Command, command);
        SimpleMapper.Map(task.CommandJob, command);

        if (!contextProvider.Context.Allows(permissionId, command.SchemaId.Name))
        {
            throw new DomainForbiddenException("Forbidden");
        }

        command.SchemaId = task.SchemaId;
        command.ExpectedVersion = task.Command.ExpectedVersion;
    }

    private async Task<DomainId[]> FindIdAsync(BulkTask task, string schema)
    {
        var id = task.CommandJob.Id;

        if (id != null)
        {
            return new[] { id.Value };
        }

        if (task.CommandJob.Query != null)
        {
            task.CommandJob.Query.Take = task.CommandJob.ExpectedCount;

            var existingQuery = Q.Empty.WithJsonQuery(task.CommandJob.Query);
            var existingResult = await contentQuery.QueryAsync(contextProvider.Context, schema, existingQuery, task.Aborted);

            if (existingResult.Total > task.CommandJob.ExpectedCount)
            {
                throw new DomainException(T.Get("contents.bulkInsertQueryNotUnique"));
            }

            // Upsert means that we either update the content if we find it or that we create a new one.
            // Therefore we create a new ID if we cannot find the ID for the query.
            if (existingResult.Count == 0 && task.CommandJob.Type == BulkUpdateContentType.Upsert)
            {
                return new[] { DomainId.NewGuid() };
            }

            return existingResult.Select(x => x.Id).ToArray();
        }

        if (task.CommandJob.Type is BulkUpdateContentType.Create or BulkUpdateContentType.Upsert)
        {
            return new[] { DomainId.NewGuid() };
        }

        return Array.Empty<DomainId>();
    }
}
