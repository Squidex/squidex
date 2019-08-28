﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentGrain : LogSnapshotDomainObjectGrain<ContentState>, IContentGrain
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
        private readonly IAppProvider appProvider;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;
        private readonly IScriptEngine scriptEngine;
        private readonly IContentWorkflow contentWorkflow;

        public ContentGrain(
            IStore<Guid> store,
            ISemanticLog log,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            IScriptEngine scriptEngine,
            IContentWorkflow contentWorkflow,
            IContentRepository contentRepository,
            IActivationLimit limit)
            : base(store, log)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentWorkflow, nameof(contentWorkflow));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.appProvider = appProvider;
            this.scriptEngine = scriptEngine;
            this.assetRepository = assetRepository;
            this.contentWorkflow = contentWorkflow;
            this.contentRepository = contentRepository;

            limit?.SetLimit(5000, Lifetime);
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            VerifyNotDeleted();

            switch (command)
            {
                case CreateContent createContent:
                    return CreateReturnAsync(createContent, async c =>
                    {
                        var ctx = await CreateContext(c.AppId.Id, c.SchemaId.Id, c, () => "Failed to create content.");

                        var status = (await contentWorkflow.GetInitialStatusAsync(ctx.Schema)).Status;

                        await GuardContent.CanCreate(ctx.Schema, contentWorkflow, c);

                        c.Data = await ctx.ExecuteScriptAndTransformAsync(s => s.Create,
                            new ScriptContext
                            {
                                Operation = "Create",
                                Data = c.Data,
                                Status = status,
                                StatusOld = default
                            });

                        await ctx.EnrichAsync(c.Data);

                        if (!c.DoNotValidate)
                        {
                            await ctx.ValidateAsync(c.Data);
                        }

                        if (c.Publish)
                        {
                            await ctx.ExecuteScriptAsync(s => s.Change,
                                new ScriptContext
                                {
                                    Operation = "Published",
                                    Data = c.Data,
                                    Status = Status.Published,
                                    StatusOld = status
                                });
                        }

                        Create(c, status);

                        return Snapshot;
                    });

                case UpdateContent updateContent:
                    return UpdateReturnAsync(updateContent, async c =>
                    {
                        var isProposal = c.AsDraft && Snapshot.Status == Status.Published;

                        await GuardContent.CanUpdate(Snapshot, contentWorkflow, c, isProposal);

                        return await UpdateAsync(c, x => c.Data, false, isProposal);
                    });

                case PatchContent patchContent:
                    return UpdateReturnAsync(patchContent, async c =>
                    {
                        var isProposal = IsProposal(c);

                        await GuardContent.CanPatch(Snapshot, contentWorkflow, c, isProposal);

                        return await UpdateAsync(c, c.Data.MergeInto, true, isProposal);
                    });

                case ChangeContentStatus changeContentStatus:
                    return UpdateReturnAsync(changeContentStatus, async c =>
                    {
                        try
                        {
                            var isChangeConfirm = IsConfirm(c);

                            var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, c, () => "Failed to change content.");

                            await GuardContent.CanChangeStatus(ctx.Schema, Snapshot, contentWorkflow, c, isChangeConfirm);

                            if (c.DueTime.HasValue)
                            {
                                ScheduleStatus(c);
                            }
                            else
                            {
                                if (isChangeConfirm)
                                {
                                    ConfirmChanges(c);
                                }
                                else
                                {
                                    var change = GetChange(c);

                                    await ctx.ExecuteScriptAsync(s => s.Change,
                                        new ScriptContext
                                        {
                                            Operation = change.ToString(),
                                            Data = Snapshot.Data,
                                            Status = c.Status,
                                            StatusOld = Snapshot.Status
                                        });

                                    ChangeStatus(c, change);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (c.JobId.HasValue && Snapshot?.ScheduleJob.Id == c.JobId)
                            {
                                CancelScheduling(c);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        return Snapshot;
                    });

                case DiscardChanges discardChanges:
                    return UpdateReturn(discardChanges, c =>
                    {
                        GuardContent.CanDiscardChanges(Snapshot.IsPending, c);

                        DiscardChanges(c);

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
                                Data = Snapshot.Data,
                                Status = Snapshot.Status,
                                StatusOld = default
                            });

                        Delete(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<object> UpdateAsync(ContentUpdateCommand command, Func<NamedContentData, NamedContentData> newDataFunc, bool partial, bool isProposal)
        {
            var currentData =
                isProposal ?
                Snapshot.DataDraft :
                Snapshot.Data;

            var newData = newDataFunc(currentData);

            if (!currentData.Equals(newData))
            {
                var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, command, () => "Failed to update content.");

                if (partial)
                {
                    await ctx.ValidatePartialAsync(command.Data);
                }
                else
                {
                    await ctx.ValidateAsync(command.Data);
                }

                newData = await ctx.ExecuteScriptAndTransformAsync(s => s.Update,
                    new ScriptContext
                    {
                        Operation = "Create",
                        Data = newData,
                        DataOld = currentData,
                        Status = Snapshot.Status,
                        StatusOld = default
                    });

                if (isProposal)
                {
                    ProposeUpdate(command, newData);
                }
                else
                {
                    Update(command, newData);
                }
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

        public void ConfirmChanges(ChangeContentStatus command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentChangesPublished()));
        }

        public void DiscardChanges(DiscardChanges command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentChangesDiscarded()));
        }

        public void Delete(DeleteContent command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentDeleted()));
        }

        public void Update(ContentCommand command, NamedContentData data)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentUpdated { Data = data }));
        }

        public void ProposeUpdate(ContentCommand command, NamedContentData data)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentUpdateProposed { Data = data }));
        }

        public void CancelScheduling(ChangeContentStatus command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentSchedulingCancelled()));
        }

        public void ScheduleStatus(ChangeContentStatus command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentStatusScheduled { DueTime = command.DueTime.Value }));
        }

        public void ChangeStatus(ChangeContentStatus command, StatusChange change)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged { Change = change }));
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

        private bool IsConfirm(ChangeContentStatus command)
        {
            return Snapshot.IsPending && Snapshot.Status == Status.Published && command.Status == Status.Published;
        }

        private bool IsProposal(PatchContent command)
        {
            return Snapshot.Status == Status.Published && command.AsDraft;
        }

        private StatusChange GetChange(ChangeContentStatus command)
        {
            var change = StatusChange.Change;

            if (command.Status == Status.Published)
            {
                change = StatusChange.Published;
            }
            else if (Snapshot.Status == Status.Published)
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

        public Task<J<IContentEntity>> GetStateAsync(long version = EtagVersion.Any)
        {
            return J.AsTask<IContentEntity>(GetSnapshot(version));
        }
    }
}
