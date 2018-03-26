// ==========================================================================
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
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentGrain : SquidexDomainObjectGrain<ContentState>, IContentGrain
    {
        private readonly IAppProvider appProvider;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;
        private readonly IScriptEngine scriptEngine;

        public ContentGrain(
            IStore<Guid> store,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            IScriptEngine scriptEngine,
            IContentRepository contentRepository)
            : base(store)
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
                        GuardContent.CanCreate(c);

                        var operationContext = await CreateContext(c.AppId.Id, c.SchemaId.Id, () => "Failed to create content.");

                        await operationContext.ExecuteScriptAndTransformAsync(x => x.ScriptCreate, "Create", c, c.Data, null);
                        await operationContext.EnrichAsync(c.Data);
                        await operationContext.ValidateAsync(c.Data);

                        if (c.Publish)
                        {
                            await operationContext.ExecuteScriptAsync(x => x.ScriptChange, "Published", c, c.Data, null);
                        }

                        Create(c);

                        return EntityCreatedResult.Create(c.Data, NewVersion);
                    });

                case UpdateContent updateContent:
                    return UpdateReturnAsync(updateContent, c =>
                    {
                        GuardContent.CanUpdate(c);

                        return UpdateContentAsync(c, c.Data, "Update", c.AsProposal);
                    });

                case PatchContent patchContent:
                    return UpdateReturnAsync(patchContent, c =>
                    {
                        GuardContent.CanPatch(c);

                        return UpdateContentAsync(c, c.Data.MergeInto(Snapshot.PendingData ?? Snapshot.Data), "Patch", c.AsProposal);
                    });

                case ChangeContentStatus changeContentStatus:
                    return UpdateReturnAsync(changeContentStatus, c =>
                    {
                        GuardContent.CanChangeContentStatus(Snapshot.PendingData, Snapshot.Status, c);

                        if (Snapshot.PendingData != null)
                        {
                            return UpdateContentAsync(c, Snapshot.PendingData, "Update", false);
                        }
                        else
                        {
                            return ChangeStatusAsync(c);
                        }
                    });

                case DeleteContent deleteContent:
                    return UpdateAsync(deleteContent, async c =>
                    {
                        GuardContent.CanDelete(c);

                        var operationContext = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, () => "Failed to delete content.");

                        await operationContext.ExecuteScriptAsync(x => x.ScriptDelete, "Delete", c, Snapshot.Data);

                        Delete(c);
                    });

                case DiscardChanges discardChanges:
                    return UpdateAsync(discardChanges, c =>
                    {
                        GuardContent.CanDiscardChanges(Snapshot.PendingData, c);

                        DiscardChanges(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<object> ChangeStatusAsync(ChangeContentStatus c)
        {
            if (!c.DueTime.HasValue)
            {
                var operationContext = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, () => "Failed to change content.");

                await operationContext.ExecuteScriptAsync(x => x.ScriptChange, c.Status, c, Snapshot.Data);
            }

            ChangeStatus(c);

            return new EntitySavedResult(NewVersion);
        }

        private async Task<object> UpdateContentAsync(ContentCommand command, NamedContentData data, string operation, bool asProposal)
        {
            var operationContext = await CreateContext(Snapshot.AppId.Id, Snapshot.SchemaId.Id, () => "Failed to update content.");

            if (!Snapshot.Data.Equals(data))
            {
                await operationContext.ValidateAsync(data);

                if (asProposal)
                {
                    ProposeUpdate(command, data);
                }
                else
                {
                    await operationContext.ExecuteScriptAndTransformAsync(x => x.ScriptUpdate, "Update", command, data, Snapshot.Data);

                    Update(command, data);
                }
            }

            return new ContentDataChangedResult(data, NewVersion);
        }

        public void Create(CreateContent command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentCreated()));

            if (command.Publish)
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged { Status = Status.Published }));
            }
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

        public void ChangeStatus(ChangeContentStatus command)
        {
            if (command.DueTime.HasValue)
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentStatusScheduled { DueTime = command.DueTime.Value }));
            }
            else
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged()));
            }
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

        public override void ApplyEvent(Envelope<IEvent> @event)
        {
            ApplySnapshot(Snapshot.Apply(@event));
        }

        private async Task<ContentOperationContext> CreateContext(Guid appId, Guid schemaId, Func<string> message)
        {
            var operationContext =
                await ContentOperationContext.CreateAsync(appId, schemaId,
                    appProvider,
                    assetRepository,
                    contentRepository,
                    scriptEngine,
                    message);

            return operationContext;
        }
    }
}
