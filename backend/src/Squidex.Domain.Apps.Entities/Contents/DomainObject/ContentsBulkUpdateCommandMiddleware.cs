// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public sealed class ContentsBulkUpdateCommandMiddleware : ICommandMiddleware
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContextProvider contextProvider;

        private sealed record BulkTaskCommand(BulkTask Task, DomainId Id, ICommand Command)
        {
        }

        private sealed record BulkTask(
            ICommandBus Bus,
            string Schema,
            int JobIndex,
            BulkUpdateJob CommandJob,
            BulkUpdateContents Command,
            ConcurrentBag<BulkUpdateResultItem> Results
        )
        {
        }

        public ContentsBulkUpdateCommandMiddleware(IContentQueryService contentQuery, IContextProvider contextProvider)
        {
            this.contentQuery = contentQuery;
            this.contextProvider = contextProvider;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is BulkUpdateContents bulkUpdates)
            {
                if (bulkUpdates.Jobs?.Length > 0)
                {
                    var executionOptions = new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
                    };

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

                    var requestedSchema = bulkUpdates.SchemaId.Name;

                    var results = new ConcurrentBag<BulkUpdateResultItem>();

                    for (var i = 0; i < bulkUpdates.Jobs.Length; i++)
                    {
                        var task = new BulkTask(
                            context.CommandBus,
                            requestedSchema,
                            i,
                            bulkUpdates.Jobs[i],
                            bulkUpdates,
                            results);

                        if (!await createCommandsBlock.SendAsync(task))
                        {
                            break;
                        }
                    }

                    createCommandsBlock.Complete();

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
                await next(context);
            }
        }

        private static async Task ExecuteCommandAsync(BulkTaskCommand bulkCommand)
        {
            var (task, id, command) = bulkCommand;

            try
            {
                await task.Bus.PublishAsync(command);

                task.Results.Add(new BulkUpdateResultItem(id, task.JobIndex));
            }
            catch (Exception ex)
            {
                task.Results.Add(new BulkUpdateResultItem(id, task.JobIndex, ex));
            }
        }

        private async Task<IEnumerable<BulkTaskCommand>> CreateCommandsAsync(BulkTask task)
        {
            var commands = new List<BulkTaskCommand>();

            try
            {
                var resolvedIds = await FindIdAsync(task);

                if (resolvedIds.Length == 0)
                {
                    throw new DomainObjectNotFoundException("undefined");
                }

                foreach (var id in resolvedIds)
                {
                    try
                    {
                        var command = await CreateCommandAsync(task);

                        command.ContentId = id;

                        commands.Add(new BulkTaskCommand(task, id, command));
                    }
                    catch (Exception ex)
                    {
                        task.Results.Add(new BulkUpdateResultItem(id, task.JobIndex, ex));
                    }
                }
            }
            catch (Exception ex)
            {
                task.Results.Add(new BulkUpdateResultItem(null, task.JobIndex, ex));
            }

            return commands;
        }

        private async Task<ContentCommand> CreateCommandAsync(BulkTask task)
        {
            var job = task.CommandJob;

            switch (job.Type)
            {
                case BulkUpdateContentType.Create:
                    {
                        var command = new CreateContent();

                        await EnrichAndCheckPermissionAsync(task, command, Permissions.AppContentsCreate);
                        return command;
                    }

                case BulkUpdateContentType.Update:
                    {
                        var command = new UpdateContent();

                        await EnrichAndCheckPermissionAsync(task, command, Permissions.AppContentsUpdateOwn);
                        return command;
                    }

                case BulkUpdateContentType.Upsert:
                    {
                        var command = new UpsertContent();

                        await EnrichAndCheckPermissionAsync(task, command, Permissions.AppContentsUpsert);
                        return command;
                    }

                case BulkUpdateContentType.Patch:
                    {
                        var command = new PatchContent();

                        await EnrichAndCheckPermissionAsync(task, command, Permissions.AppContentsUpdateOwn);
                        return command;
                    }

                case BulkUpdateContentType.Validate:
                    {
                        var command = new ValidateContent();

                        await EnrichAndCheckPermissionAsync(task, command, Permissions.AppContentsReadOwn);
                        return command;
                    }

                case BulkUpdateContentType.ChangeStatus:
                    {
                        var command = new ChangeContentStatus { Status = job.Status ?? Status.Draft };

                        await EnrichAndCheckPermissionAsync(task, command, Permissions.AppContentsChangeStatusOwn);
                        return command;
                    }

                case BulkUpdateContentType.Delete:
                    {
                        var command = new DeleteContent();

                        await EnrichAndCheckPermissionAsync(task, command, Permissions.AppContentsDeleteOwn);
                        return command;
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task EnrichAndCheckPermissionAsync<T>(BulkTask task, T command, string permissionId) where T : ContentCommand
        {
            SimpleMapper.Map(task.Command, command);
            SimpleMapper.Map(task.CommandJob, command);

            if (!string.IsNullOrWhiteSpace(task.CommandJob.Schema))
            {
                var schema = await contentQuery.GetSchemaOrThrowAsync(contextProvider.Context, task.Schema);

                command.SchemaId = schema.NamedId();
            }

            if (!contextProvider.Context.Allows(permissionId, command.SchemaId.Name))
            {
                throw new DomainForbiddenException("Forbidden");
            }

            command.ExpectedVersion = task.Command.ExpectedVersion;
        }

        private async Task<DomainId[]> FindIdAsync(BulkTask task)
        {
            var id = task.CommandJob.Id;

            if (id != null)
            {
                return new[] { id.Value };
            }

            if (task.CommandJob.Query != null)
            {
                task.CommandJob.Query.Take = task.CommandJob.ExpectedCount;

                var existing = await contentQuery.QueryAsync(contextProvider.Context, task.Schema, Q.Empty.WithJsonQuery(task.CommandJob.Query));

                if (existing.Total > task.CommandJob.ExpectedCount)
                {
                    throw new DomainException(T.Get("contents.bulkInsertQueryNotUnique"));
                }

                return existing.Select(x => x.Id).ToArray();
            }

            if (task.CommandJob.Type == BulkUpdateContentType.Create || task.CommandJob.Type == BulkUpdateContentType.Upsert)
            {
                return new[] { DomainId.NewGuid() };
            }

            return Array.Empty<DomainId>();
        }
    }
}
