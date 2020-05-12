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
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class BulkUpdateCommandMiddleware : ICommandMiddleware
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IContentQueryService contentQuery;
        private readonly IContextProvider contextProvider;

        public BulkUpdateCommandMiddleware(IServiceProvider serviceProvider, IContentQueryService contentQuery, IContextProvider contextProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(contextProvider, nameof(contextProvider));

            this.serviceProvider = serviceProvider;
            this.contentQuery = contentQuery;
            this.contextProvider = contextProvider;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is BulkUpdateContents bulkUpdates)
            {
                if (bulkUpdates.Jobs?.Count > 0)
                {
                    var requestContext = contextProvider.Context.WithoutContentEnrichment().WithUnpublished(true);
                    var requestedSchema = bulkUpdates.SchemaId.Name;

                    var results = new BulkUpdateResultItem[bulkUpdates.Jobs.Count];

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
                                        if (id.HasValue)
                                        {
                                            var command = SimpleMapper.Map(bulkUpdates, new UpdateContent { Data = job.Data, ContentId = id.Value });

                                            await context.CommandBus.PublishAsync(command);

                                            results[index] = new BulkUpdateResultItem { ContentId = id };
                                        }
                                        else
                                        {
                                            var command = SimpleMapper.Map(bulkUpdates, new CreateContent { Data = job.Data });

                                            var content = serviceProvider.GetRequiredService<ContentDomainObject>();

                                            content.Setup(command.ContentId);

                                            await content.ExecuteAsync(command);

                                            result.ContentId = command.ContentId;
                                        }

                                        break;
                                    }

                                case BulkUpdateType.ChangeStatus:
                                    {
                                        if (id == null || id == default)
                                        {
                                            throw new DomainObjectNotFoundException("NOT DEFINED", typeof(IContentEntity));
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
                                        if (id == null || id == default)
                                        {
                                            throw new DomainObjectNotFoundException("NOT DEFINED", typeof(IContentEntity));
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

                    for (var i = 0; i < bulkUpdates.Jobs.Count; i++)
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

        private async Task<Guid?> FindIdAsync(Context context, string schema, BulkUpdateJob job)
        {
            var id = job.Id;

            if (id == null && job.Query != null)
            {
                job.Query.Take = 1;

                var existing = await contentQuery.QueryAsync(context, schema, Q.Empty.WithJsonQuery(job.Query));

                if (existing.Total > 1)
                {
                    throw new DomainException("More than one content matches to the query.");
                }

                id = existing.FirstOrDefault()?.Id;
            }

            return id;
        }
    }
}
