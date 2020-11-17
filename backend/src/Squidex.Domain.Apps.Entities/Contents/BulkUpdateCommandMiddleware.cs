// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Translations;

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class BulkUpdateCommandMiddleware : ICommandMiddleware
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContextProvider contextProvider;

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
                    var requestContext = contextProvider.Context.WithoutContentEnrichment().WithUnpublished(true);
                    var requestedSchema = bulkUpdates.SchemaId.Name;

                    var results = new BulkUpdateResultItem[bulkUpdates.Jobs.Length];

                    var actionBlock = new ActionBlock<int>(async index =>
                    {
                        var job = bulkUpdates.Jobs[index];

                        var result = new BulkUpdateResultItem();

                        try
                        {
                            var id = await FindIdAsync(requestContext, requestedSchema, job);

                            result.ContentId = id;

                            switch (job.Type)
                            {
                                case BulkUpdateType.Upsert:
                                    {
                                        var command = SimpleMapper.Map(bulkUpdates, new UpsertContent { Data = job.Data });

                                        if (id != null && id != DomainId.Empty)
                                        {
                                            command.ContentId = id.Value;
                                        }

                                        result.ContentId = command.ContentId;

                                        await context.CommandBus.PublishAsync(command);
                                        break;
                                    }

                                case BulkUpdateType.ChangeStatus:
                                    {
                                        if (id == null || id == DomainId.Empty)
                                        {
                                            throw new DomainObjectNotFoundException("undefined");
                                        }

                                        var command = SimpleMapper.Map(bulkUpdates, new ChangeContentStatus { ContentId = id.Value });

                                        if (job.Status != null)
                                        {
                                            command.Status = job.Status.Value;
                                        }

                                        await context.CommandBus.PublishAsync(command);
                                        break;
                                    }

                                case BulkUpdateType.Delete:
                                    {
                                        if (id == null || id == DomainId.Empty)
                                        {
                                            throw new DomainObjectNotFoundException("undefined");
                                        }

                                        var command = SimpleMapper.Map(bulkUpdates, new DeleteContent { ContentId = id.Value });

                                        await context.CommandBus.PublishAsync(command);
                                        break;
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Exception = ex;
                        }

                        results[index] = result;
                    }, new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
                    });

                    for (var i = 0; i < bulkUpdates.Jobs.Length; i++)
                    {
                        await actionBlock.SendAsync(i);
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

        private async Task<DomainId?> FindIdAsync(Context context, string schema, BulkUpdateJob job)
        {
            var id = job.Id;

            if (id == null && job.Query != null)
            {
                job.Query.Take = 1;

                var existing = await contentQuery.QueryAsync(context, schema, Q.Empty.WithJsonQuery(job.Query));

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
