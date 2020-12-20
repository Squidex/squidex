// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class BulkUpdateCommandMiddleware : ICommandMiddleware
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContextProvider contextProvider;

        private sealed record BulkTask(
            ICommandBus Bus,
            Context Context,
            string Schema,
            BulkUpdateJob Job,
            BulkUpdateContents Command
        )
        {
            public BulkUpdateResultItem Result { get; } = new BulkUpdateResultItem();
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
                    var actionBlock = new ActionBlock<BulkTask>(async task =>
                    {
                        try
                        {
                            await ExecuteTaskAsync(task);
                        }
                        catch (Exception ex)
                        {
                            task.Result.Exception = ex;
                        }
                    }, new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
                    });

                    var requestContext = contextProvider.Context.WithoutContentEnrichment().WithUnpublished(true);
                    var requestedSchema = bulkUpdates.SchemaId.Name;

                    var results = new List<BulkUpdateResultItem>(bulkUpdates.Jobs.Length);

                    for (var i = 0; i < bulkUpdates.Jobs.Length; i++)
                    {
                        var task = new BulkTask(
                            context.CommandBus,
                            requestContext,
                            requestedSchema,
                            bulkUpdates.Jobs[i],
                            bulkUpdates);

                        await actionBlock.SendAsync(task);

                        results.Add(task.Result);
                    }

                    actionBlock.Complete();

                    await actionBlock.Completion;

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

        private async Task ExecuteTaskAsync(BulkTask task)
        {
            var job = task.Job;

            var resolvedId = await FindIdAsync(task);

            DomainId id;

            if (resolvedId == null || resolvedId == DomainId.Empty)
            {
                if (job.Type == BulkUpdateType.Upsert)
                {
                    id = DomainId.NewGuid();
                }
                else
                {
                    throw new DomainObjectNotFoundException("undefined");
                }
            }
            else
            {
                id = resolvedId.Value;
            }

            task.Result.ContentId = id;

            switch (job.Type)
            {
                case BulkUpdateType.Upsert:
                    {
                        var command = new UpsertContent { Data = job.Data! };

                        await PublishAsync(id, task, command, Permissions.AppContentsUpsert);
                        break;
                    }

                case BulkUpdateType.Validate:
                    {
                        var command = new ValidateContent();

                        await PublishAsync(id, task, command, Permissions.AppContentsRead);
                        break;
                    }

                case BulkUpdateType.ChangeStatus:
                    {
                        var command = new ChangeContentStatus { Status = job.Status, DueTime = job.DueTime };

                        await PublishAsync(id, task, command, Permissions.AppContentsUpdate);
                        break;
                    }

                case BulkUpdateType.Delete:
                    {
                        var command = new DeleteContent();

                        await PublishAsync(id, task, command, Permissions.AppContentsDelete);
                        break;
                    }
            }
        }

        private async Task PublishAsync<TCommand>(DomainId id, BulkTask task, TCommand command, string permissionId) where TCommand : ContentCommand
        {
            SimpleMapper.Map(task.Command, command);

            command.ContentId = id;

            if (!string.IsNullOrWhiteSpace(task.Job.Schema))
            {
                var schema = await contentQuery.GetSchemaOrThrowAsync(task.Context, task.Schema);

                command.SchemaId = schema.NamedId();
            }

            var permission = Permissions.ForApp(permissionId, command.AppId.Name, command.SchemaId.Name);

            if (!task.Context.Permissions.Allows(permission))
            {
                throw new DomainForbiddenException("Forbidden");
            }

            command.ExpectedVersion = task.Command.ExpectedVersion;

            await task.Bus.PublishAsync(command);
        }

        private async Task<DomainId?> FindIdAsync(BulkTask task)
        {
            var id = task.Job.Id;

            if (id == null && task.Job.Query != null)
            {
                task.Job.Query.Take = 1;

                var existing = await contentQuery.QueryAsync(task.Context, task.Schema, Q.Empty.WithJsonQuery(task.Job.Query));

                if (existing.Total > 1)
                {
                    throw new DomainException(T.Get("contents.bulkInsertQueryNotUnique"));
                }

                id = existing.FirstOrDefault()?.Id;
            }

            return id;
        }
    }
}
