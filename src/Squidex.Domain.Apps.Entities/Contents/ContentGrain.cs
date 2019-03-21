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
    public sealed class ContentGrain : SquidexDomainObjectGrainLogSnapshots<ContentState>, IContentGrain
    {
        private readonly IAppProvider appProvider;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;
        private readonly IScriptEngine scriptEngine;

        public ContentGrain(
            IStore<Guid> store,
            ISemanticLog log,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            IScriptEngine scriptEngine,
            IContentRepository contentRepository)
            : base(store, log)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.appProvider = appProvider;
            this.scriptEngine = scriptEngine;
            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            VerifyNotDeleted();

            switch (command)
            {
                case CreateContent createContent:
                    return CreateReturnAsync(createContent, async c =>
                    {
                        var ctx = await CreateContext(c.AppId.Id, c.SchemaId.Id, Guid.Empty, () => "Failed to create content.");

                        GuardContent.CanCreate(ctx.Schema, c);

                        await ctx.ExecuteScriptAndTransformAsync(s => s.Create, "Create", c, c.Data);
                        await ctx.EnrichAsync(c.Data);

                        if (!c.DoNotValidate)
                        {
                            await ctx.ValidateAsync(c.Data);
                        }

                        if (c.Publish)
                        {
                            await ctx.ExecuteScriptAsync(s => s.Change, "Published", c, c.Data);
                        }

                        Create(c);

                        return EntityCreatedResult.Create(c.Data, Version);
                    });

                case UpdateContent updateContent:
                    return UpdateReturnAsync(updateContent, c =>
                    {
                        GuardContent.CanUpdate(c);

                        return UpdateAsync(c, x => c.Data, false);
                    });

                case UpdateContentOrderNo updateContentOrderNo:
                    return UpdateReturnAsync(updateContentOrderNo, async c =>
                    {
                        var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, Snapshot.Id, () => "Failed to update sorting.");
                        UpdateOrderNo(updateContentOrderNo, c.NewOrderNo);
                        return Task.CompletedTask;
                    });

                case PatchContent patchContent:
                    return UpdateReturnAsync(patchContent, c =>
                    {
                        GuardContent.CanPatch(c);

                        return UpdateAsync(c, c.Data.MergeInto, true);
                    });

                case ChangeContentStatus changeContentStatus:
                    return UpdateAsync(changeContentStatus, async c =>
                    {
                        try
                        {
                            var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, Snapshot.Id, () => "Failed to change content.");

                            GuardContent.CanChangeContentStatus(ctx.Schema, Snapshot.IsPending, Snapshot.Status, c);

                            if (c.DueTime.HasValue)
                            {
                                ScheduleStatus(c);
                            }
                            else
                            {
                                if (Snapshot.IsPending && Snapshot.Status == Status.Published && c.Status == Status.Published)
                                {
                                    ConfirmChanges(c);
                                }
                                else
                                {
                                    StatusChange reason;

                                    if (c.Status == Status.Published)
                                    {
                                        reason = StatusChange.Published;
                                    }
                                    else if (c.Status == Status.Archived)
                                    {
                                        reason = StatusChange.Archived;
                                    }
                                    else if (Snapshot.Status == Status.Published)
                                    {
                                        reason = StatusChange.Unpublished;
                                    }
                                    else
                                    {
                                        reason = StatusChange.Restored;
                                    }

                                    await ctx.ExecuteScriptAsync(s => s.Change, reason, c, Snapshot.Data);

                                    ChangeStatus(c, reason);
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
                    });

                case DeleteContent deleteContent:
                    return UpdateAsync(deleteContent, async c =>
                    {
                        var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, Snapshot.Id, () => "Failed to delete content.");

                        GuardContent.CanDelete(ctx.Schema, c);

                        await ctx.ExecuteScriptAsync(s => s.Delete, "Delete", c, Snapshot.Data);

                        Delete(c);
                    });

                case DiscardChanges discardChanges:
                    return UpdateAsync(discardChanges, c =>
                    {
                        GuardContent.CanDiscardChanges(Snapshot.IsPending, c);

                        DiscardChanges(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<object> UpdateAsync(ContentDataCommand c, Func<NamedContentData, NamedContentData> newDataFunc, bool partial)
        {
            var isProposal = c.AsDraft && Snapshot.Status == Status.Published;

            var currentData =
                isProposal ?
                Snapshot.DataDraft :
                Snapshot.Data;

            var newData = newDataFunc(currentData);

            if (!currentData.Equals(newData))
            {
                var ctx = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, Snapshot.Id, () => "Failed to update content.");

                if (partial)
                {
                    await ctx.ValidatePartialAsync(c.Data);
                }
                else
                {
                    await ctx.ValidateAsync(c.Data);
                }

                newData = await ctx.ExecuteScriptAndTransformAsync(s => s.Update, "Update", c, newData, Snapshot.Data);

                if (isProposal)
                {
                    ProposeUpdate(c, newData);
                }
                else
                {
                    Update(c, newData);
                }
            }

            return new ContentDataChangedResult(newData, Version);
        }

        public void Create(CreateContent command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentCreated()));

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

        public void UpdateOrderNo(UpdateContentOrderNo command, long orderNo)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentOrderChanged { NewOrderNo = orderNo }));
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

        public void ChangeStatus(ChangeContentStatus command, StatusChange reason)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged { Change = reason }));
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

        private void VerifyNotDeleted()
        {
            if (Snapshot.IsDeleted)
            {
                throw new DomainException("Content has already been deleted.");
            }
        }

        protected override ContentState OnEvent(Envelope<IEvent> @event)
        {
            return Snapshot.Apply(@event);
        }

        private async Task<ContentOperationContext> CreateContext(Guid appId, Guid schemaId, Guid contentId, Func<string> message)
        {
            var operationContext =
                await ContentOperationContext.CreateAsync(appId, schemaId, contentId,
                    appProvider, assetRepository, contentRepository, scriptEngine, message);

            return operationContext;
        }

        public Task<J<IContentEntity>> GetStateAsync(long version = EtagVersion.Any)
        {
            return J.AsTask<IContentEntity>(GetSnapshot(version));
        }
    }
}
