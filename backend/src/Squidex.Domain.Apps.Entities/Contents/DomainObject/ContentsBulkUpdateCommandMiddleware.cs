// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public sealed class ContentsBulkUpdateCommandMiddleware : ICommandMiddleware
{
    private readonly IContentQueryService contentQuery;
    private readonly IContextProvider contextProvider;

    private sealed record BulkTask(
        BulkUpdateJob BulkJob,
        BulkUpdateContents Bulk,
        int JobIndex,
        ContentCommand? Command)
    {
        public BulkUpdateResultItem? Result { get; private set; }

        public BulkTask SetResult(Exception? exception = null)
        {
            var id = Command?.ContentId ?? BulkJob.Id;

            Result = new BulkUpdateResultItem(id, JobIndex, exception);
            return this;
        }

        public static BulkTask Failed(BulkUpdateJob bulkJob, BulkUpdateContents bulk, int jobIndex, Exception exception)
        {
            return new BulkTask(bulkJob, bulk, jobIndex, null).SetResult(exception);
        }
    }

    public ContentsBulkUpdateCommandMiddleware(
        IContentQueryService contentQuery,
        IContextProvider contextProvider)
    {
        this.contentQuery = contentQuery;
        this.contextProvider = contextProvider;
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is not BulkUpdateContents bulkUpdates)
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
            .WithNoEnrichment()
            .WithNoCleanup()
            .WithUnpublished(true)
            .WithNoTotal());

        var tasks = await bulkUpdates.Jobs.SelectManyAsync((job, i, ct) => CreateTasksAsync(job, bulkUpdates, i, ct), ct);

        // Group the items by id, so that we do not run jobs in parallel on the same entity.
        var groupedTasks = tasks.GroupBy(x => x.Command?.ContentId).ToList();

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

    private async Task<IEnumerable<BulkTask>> CreateTasksAsync(
        BulkUpdateJob bulkJob,
        BulkUpdateContents bulk,
        int jobIndex,
        CancellationToken ct)
    {
        // The task parallel pipeline does not allow async-enumerable.
        var tasks = new List<BulkTask>();
        try
        {
            var schemaId = bulk.SchemaId;

            // Check whether another schema is defined for the current job and override the schema id if necessary.
            if (!string.IsNullOrWhiteSpace(bulkJob.Schema))
            {
                var schema = await contentQuery.GetSchemaOrThrowAsync(contextProvider.Context, bulkJob.Schema, ct);

                schemaId = schema.NamedId();
            }

            // The bulk command can be invoke in a schema controller or without a schema controller, therefore the name might be null.
            if (schemaId == null || schemaId.Id == default)
            {
                tasks.Add(BulkTask.Failed(bulkJob, bulk, jobIndex, new DomainObjectNotFoundException("undefined")));
                return tasks;
            }

            var resolvedIds = await FindIdAsync(schemaId, bulkJob, ct);

            if (resolvedIds.Length == 0)
            {
                tasks.Add(BulkTask.Failed(bulkJob, bulk, jobIndex, new DomainObjectNotFoundException("undefined")));
                return tasks;
            }

            foreach (var id in resolvedIds)
            {
                tasks.Add(CreateTask(id, schemaId, bulkJob, bulk, jobIndex));
            }
        }
        catch (Exception ex)
        {
            tasks.Add(BulkTask.Failed(bulkJob, bulk, jobIndex, ex));
        }

        return tasks;
    }

    private BulkTask CreateTask(
        DomainId id,
        NamedId<DomainId> schemaId,
        BulkUpdateJob bulkJob,
        BulkUpdateContents bulk,
        int jobIndex)
    {
        try
        {
            switch (bulkJob.Type)
            {
                case BulkUpdateContentType.Create:
                    return CreateTask<CreateContent>(id, schemaId, bulkJob, bulk, jobIndex,
                        PermissionIds.AppContentsCreate);

                case BulkUpdateContentType.Update:
                    return CreateTask<UpdateContent>(id, schemaId, bulkJob, bulk, jobIndex,
                        PermissionIds.AppContentsUpdateOwn);

                case BulkUpdateContentType.Upsert:
                    return CreateTask<UpsertContent>(id, schemaId, bulkJob, bulk, jobIndex,
                        PermissionIds.AppContentsUpsert);

                case BulkUpdateContentType.Patch:
                    return CreateTask<PatchContent>(id, schemaId, bulkJob, bulk, jobIndex,
                        PermissionIds.AppContentsUpdateOwn);

                case BulkUpdateContentType.Validate:
                    return CreateTask<ValidateContent>(id, schemaId, bulkJob, bulk, jobIndex,
                        PermissionIds.AppContentsReadOwn);

                case BulkUpdateContentType.EnrichDefaults:
                    return CreateTask<EnrichContentDefaults>(id, schemaId, bulkJob, bulk, jobIndex,
                        PermissionIds.AppContentsUpdateOwn);

                case BulkUpdateContentType.ChangeStatus:
                    return CreateTask<ChangeContentStatus>(id, schemaId, bulkJob, bulk, jobIndex,
                        PermissionIds.AppContentsChangeStatusOwn);

                case BulkUpdateContentType.Delete:
                    return CreateTask<DeleteContent>(id, schemaId, bulkJob, bulk, jobIndex,
                        PermissionIds.AppContentsDeleteOwn);

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
        DomainId id,
        NamedId<DomainId> schemaId,
        BulkUpdateJob bulkJob,
        BulkUpdateContents bulk,
        int jobIndex,
        string permissionId) where T : ContentCommand, new()
    {
        if (!contextProvider.Context.Allows(permissionId, schemaId.Name))
        {
            return BulkTask.Failed(bulkJob, bulk, jobIndex, new DomainForbiddenException("Forbidden"));
        }

        var command = new T();

        SimpleMapper.Map(bulk, command);
        SimpleMapper.Map(bulkJob, command);

        command.ContentId = id;
        command.SchemaId = schemaId;

        return new BulkTask(bulkJob, bulk, jobIndex, command);
    }

    private async Task<DomainId[]> FindIdAsync(NamedId<DomainId> schemaId, BulkUpdateJob bulkJob,
        CancellationToken ct)
    {
        var id = bulkJob.Id;

        if (id != null)
        {
            return [id.Value];
        }

        if (bulkJob.Query != null)
        {
            bulkJob.Query.Take = bulkJob.ExpectedCount;

            var existingQuery = Q.Empty.WithJsonQuery(bulkJob.Query);
            var existingResult = await contentQuery.QueryAsync(contextProvider.Context, schemaId.Id.ToString(), existingQuery, ct);

            if (existingResult.Total > bulkJob.ExpectedCount)
            {
                throw new DomainException(T.Get("contents.bulkInsertQueryNotUnique"));
            }

            // Upsert means that we either update the content if we find it or that we create a new one.
            // Therefore we create a new ID if we cannot find the ID for the query.
            if (existingResult.Count == 0 && bulkJob.Type == BulkUpdateContentType.Upsert)
            {
                return [DomainId.NewGuid()];
            }

            return existingResult.Select(x => x.Id).ToArray();
        }

        if (bulkJob.Type is BulkUpdateContentType.Create or BulkUpdateContentType.Upsert)
        {
            return [DomainId.NewGuid()];
        }

        return [];
    }
}
