// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Guards;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentDomainObject : LogSnapshotDomainObject<ContentState>
    {
        private readonly IAppProvider appProvider;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;
        private readonly IScriptEngine scriptEngine;
        private readonly IContentWorkflow contentWorkflow;

        public ContentDomainObject(
            IStore<Guid> store,
            ISemanticLog log,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            IScriptEngine scriptEngine,
            IContentWorkflow contentWorkflow,
            IContentRepository contentRepository)
            : base(store, log)
        {
            Guard.NotNull(appProvider);
            Guard.NotNull(scriptEngine);
            Guard.NotNull(assetRepository);
            Guard.NotNull(contentWorkflow);
            Guard.NotNull(contentRepository);

            this.appProvider = appProvider;
            this.scriptEngine = scriptEngine;
            this.assetRepository = assetRepository;
            this.contentWorkflow = contentWorkflow;
            this.contentRepository = contentRepository;
        }

        public override Task<object?> ExecuteAsync(IAggregateCommand command)
        {
            VerifyNotDeleted();

            switch (command)
            {
                case CreateContent createContent:
                    return CreateReturnAsync(createContent, async c =>
                    {
                        var ctx = await CreateContext(c.AppId.Id, c.SchemaId.Id, c, () => "Failed to create content.");

                        await GuardContent.CanCreate(ctx.Schema, contentWorkflow, c);

                        var status = await contentWorkflow.GetInitialStatusAsync(ctx.Schema);

                        if (!c.DoNotScript)
                        {
                            c.Data = await ctx.ExecuteScriptAndTransformAsync(s => s.Create,
                                new ScriptContext
                                {
                                    Operation = "Create",
                                    Data = c.Data,
                                    Status = status,
                                    StatusOld = default
                                });
                        }

                        await ctx.GenerateDefaultValuesAsync(c.Data);

                        if (!c.DoNotValidate)
                        {
                            await ctx.ValidateAsync(c.Data, c.OptimizeValidation);
                        }

                        if (c.Publish)
                        {
                            await ctx.ExecuteScriptAsync(s => s.Change,
                                new ScriptContext
                                {
                                    Operation = "Published",
                                    Data = c.Data,
                                    Status = Status.Published,
                                    StatusOld = default
                                });
                        }

                        Create(c, status);

                        return Snapshot;
                    });

                case CreateContentDraft createContentDraft:
                    return UpdateReturnAsync(createContentDraft, async c =>
                    {
                        var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, c, () => "Failed to create draft.");

                        GuardContent.CanCreateDraft(c, ctx.Schema, Snapshot);

                        var status = await contentWorkflow.GetInitialStatusAsync(ctx.Schema);

                        CreateDraft(c, status);

                        return Snapshot;
                    });

                case DeleteContentDraft deleteContentDraft:
                    return UpdateReturnAsync(deleteContentDraft, async c =>
                    {
                        var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, c, () => "Failed to delete draft.");

                        GuardContent.CanDeleteDraft(c, ctx.Schema, Snapshot);

                        DeleteDraft(c);

                        return Snapshot;
                    });

                case UpdateContent updateContent:
                    return UpdateReturnAsync(updateContent, async c =>
                    {
                        await GuardContent.CanUpdate(Snapshot, contentWorkflow, c);

                        return await UpdateAsync(c, x => c.Data, false);
                    });

                case PatchContent patchContent:
                    return UpdateReturnAsync(patchContent, async c =>
                    {
                        await GuardContent.CanPatch(Snapshot, contentWorkflow, c);

                        return await UpdateAsync(c, c.Data.MergeInto, true);
                    });

                case ChangeContentStatus changeContentStatus:
                    return UpdateReturnAsync(changeContentStatus, async c =>
                    {
                        try
                        {
                            var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, c, () => "Failed to change content.");

                            await GuardContent.CanChangeStatus(ctx.Schema, Snapshot, contentWorkflow, c);

                            if (c.DueTime.HasValue)
                            {
                                ScheduleStatus(c, c.DueTime.Value);
                            }
                            else
                            {
                                var change = GetChange(c);

                                await ctx.ExecuteScriptAsync(s => s.Change,
                                    new ScriptContext
                                    {
                                        Operation = change.ToString(),
                                        Data = Snapshot.EditingData,
                                        Status = c.Status,
                                        StatusOld = Snapshot.EditingStatus
                                    });

                                ChangeStatus(c, change);
                            }
                        }
                        catch (Exception)
                        {
                            if (Snapshot.ScheduleJob?.Id == c.JobId)
                            {
                                CancelChangeStatus(c);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        return Snapshot;
                    });

                case DeleteContent deleteContent:
                    return UpdateAsync(deleteContent, async c =>
                    {
                        var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, c, () => "Failed to delete content.");

                        GuardContent.CanDelete(ctx.Schema, c);

                        await ctx.ExecuteScriptAsync(s => s.Delete,
                            new ScriptContext
                            {
                                Operation = "Delete",
                                Data = Snapshot.EditingData,
                                Status = Snapshot.EditingStatus,
                                StatusOld = default
                            });

                        Delete(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<object> UpdateAsync(ContentUpdateCommand command, Func<NamedContentData, NamedContentData> newDataFunc, bool partial)
        {
            var currentData = Snapshot.EditingData;

            var newData = newDataFunc(currentData!);

            if (!currentData!.Equals(newData))
            {
                var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, command, () => "Failed to update content.");

                if (partial)
                {
                    await ctx.ValidatePartialAsync(command.Data, false);
                }
                else
                {
                    await ctx.ValidateAsync(command.Data, false);
                }

                newData = await ctx.ExecuteScriptAndTransformAsync(s => s.Update,
                    new ScriptContext
                    {
                        Operation = "Create",
                        Data = newData,
                        DataOld = currentData,
                        Status = Snapshot.EditingStatus,
                        StatusOld = default
                    });

                Update(command, newData);
            }

            return Snapshot;
        }

        public void Create(CreateContent command, Status status)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentCreated { Status = status }));

            if (command.Publish)
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged { Status = Status.Published, Change = StatusChange.Published }));
            }
        }

        public void CreateDraft(CreateContentDraft command, Status status)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentDraftCreated { Status = status }));
        }

        public void Delete(DeleteContent command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentDeleted()));
        }

        public void DeleteDraft(DeleteContentDraft command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentDraftDeleted()));
        }

        public void Update(ContentCommand command, NamedContentData data)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentUpdated { Data = data }));
        }

        public void ChangeStatus(ChangeContentStatus command, StatusChange change)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged { Change = change }));
        }

        public void CancelChangeStatus(ChangeContentStatus command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentSchedulingCancelled()));
        }

        public void ScheduleStatus(ChangeContentStatus command, Instant dueTime)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentStatusScheduled { DueTime = dueTime }));
        }

        private void RaiseEvent(SchemaEvent @event)
        {
            if (@event.AppId == null)
            {
                @event.AppId = Snapshot.AppId;
            }

            if (@event.SchemaId == null)
            {
                @event.SchemaId = Snapshot.SchemaId;
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private StatusChange GetChange(ChangeContentStatus command)
        {
            var change = StatusChange.Change;

            if (command.Status == Status.Published)
            {
                change = StatusChange.Published;
            }
            else if (Snapshot.EditingStatus == Status.Published)
            {
                change = StatusChange.Unpublished;
            }

            return change;
        }

        private void VerifyNotDeleted()
        {
            if (Snapshot.IsDeleted)
            {
                throw new DomainException("Content has already been deleted.");
            }
        }

        private async Task<ContentOperationContext> CreateContext(Guid appId, Guid schemaId, ContentCommand command, Func<string> message)
        {
            var operationContext =
                await ContentOperationContext.CreateAsync(appId, schemaId, command,
                    appProvider, assetRepository, contentRepository, scriptEngine, message);

            return operationContext;
        }
    }
}
