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
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class BulkUpdateCommandMiddleware : ICommandMiddleware
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContextProvider contextProvider;

        private sealed record BulkTaskCommand(BulkTask Task, DomainId Id, ICommand Command)
        {
        }

        private sealed record BulkTask(
            ICommandBus Bus,
            Context Context,
            string Schema,
            int JobIndex,
            BulkUpdateJob Job,
            BulkUpdateContents Command,
            ConcurrentBag<BulkUpdateResultItem> Results
        )
        {
        }

        public BulkUpdateCommandMiddleware(IContentQueryService contentQuery, IContextProvider contextProvider)
        {
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(contextProvider, nameof(contextProvider));

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
                        return await CreateCommandsAsync(task);
                    }, executionOptions);

                    var executeCommandBlock = new ActionBlock<BulkTaskCommand>(async command =>
                    {
                        await ExecuteCommandAsync(command);
                    }, executionOptions);

                    createCommandsBlock.LinkTo(executeCommandBlock, new DataflowLinkOptions
                    {
                        PropagateCompletion = true
                    });

                    var requestContext = contextProvider.Context.Clone(b => b
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
                            requestContext,
                            requestedSchema,
                            i,
                            bulkUpdates.Jobs[i],
                            bulkUpdates,
                            results);

                        await createCommandsBlock.SendAsync(task);
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

            Exception? exception = null;
            try
            {
                await task.Bus.PublishAsync(command);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            task.Results.Add(new BulkUpdateResultItem
            {
                ContentId = id,
                JobIndex = task.JobIndex,
                Exception = exception
            });
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
                        var command = await CreateCommandAsync(id, task);

                        commands.Add(new BulkTaskCommand(task, id, command));
                    }
                    catch (Exception ex)
                    {
                        task.Results.Add(new BulkUpdateResultItem
                        {
                            ContentId = id,
                            JobIndex = task.JobIndex,
                            Exception = ex
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                task.Results.Add(new BulkUpdateResultItem
                {
                    JobIndex = task.JobIndex,
                    Exception = ex
                });
            }

            return commands;
        }

        private async Task<ICommand> CreateCommandAsync(DomainId id, BulkTask task)
        {
            var job = task.Job;

            switch (job.Type)
            {
                case BulkUpdateType.Create:
                    {
                        var command = new CreateContent { Data = job.Data! };

                        await EnrichAsync(id, task, command, Permissions.AppContentsCreate);
                        return command;
                    }

                case BulkUpdateType.Update:
                    {
                        var command = new UpdateContent { Data = job.Data! };

                        await EnrichAsync(id, task, command, Permissions.AppContentsUpdateOwn);
                        return command;
                    }

                case BulkUpdateType.Upsert:
                    {
                        var command = new UpsertContent { Data = job.Data! };

                        await EnrichAsync(id, task, command, Permissions.AppContentsUpsert);
                        return command;
                    }

                case BulkUpdateType.Patch:
                    {
                        var command = new PatchContent { Data = job.Data! };

                        await EnrichAsync(id, task, command, Permissions.AppContentsUpdateOwn);
                        return command;
                    }

                case BulkUpdateType.Validate:
                    {
                        var command = new ValidateContent();

                        await EnrichAsync(id, task, command, Permissions.AppContentsReadOwn);
                        return command;
                    }

                case BulkUpdateType.ChangeStatus:
                    {
                        var command = new ChangeContentStatus { Status = job.Status, DueTime = job.DueTime };

                        await EnrichAsync(id, task, command, Permissions.AppContentsUpdateOwn);
                        return command;
                    }

                case BulkUpdateType.Delete:
                    {
                        var command = new DeleteContent();

                        await EnrichAsync(id, task, command, Permissions.AppContentsDeleteOwn);
                        return command;
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task EnrichAsync<TCommand>(DomainId id, BulkTask task, TCommand command, string permissionId) where TCommand : ContentCommand
        {
            SimpleMapper.Map(task.Command, command);

            command.ContentId = id;

            if (!string.IsNullOrWhiteSpace(task.Job.Schema))
            {
                var schema = await contentQuery.GetSchemaOrThrowAsync(task.Context, task.Schema);

                command.SchemaId = schema.NamedId();
            }

            if (!task.Context.Allows(permissionId, command.SchemaId.Name))
            {
                throw new DomainForbiddenException("Forbidden");
            }

            command.ExpectedVersion = task.Command.ExpectedVersion;
        }

        private async Task<DomainId[]> FindIdAsync(BulkTask task)
        {
            var id = task.Job.Id;

            if (id != null)
            {
                return new[] { id.Value };
            }

            if (task.Job.Query != null)
            {
                task.Job.Query.Take = task.Job.ExpectedCount;

                var existing = await contentQuery.QueryAsync(task.Context, task.Schema, Q.Empty.WithJsonQuery(task.Job.Query));

                if (existing.Total > task.Job.ExpectedCount)
                {
                    throw new DomainException(T.Get("contents.bulkInsertQueryNotUnique"));
                }

                return existing.Select(x => x.Id).ToArray();
            }

            if (task.Job.Type == BulkUpdateType.Create || task.Job.Type == BulkUpdateType.Upsert)
            {
                return new[] { DomainId.NewGuid() };
            }

            return Array.Empty<DomainId>();
        }
    }
}
