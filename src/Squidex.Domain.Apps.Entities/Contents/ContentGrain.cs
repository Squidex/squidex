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
using Squidex.Infrastructure.Log;
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
                        GuardContent.CanCreate(c);

                        var operationContext = await CreateContext(c, () => "Failed to create content.");

                        if (c.Publish)
                        {
                            await operationContext.ExecuteScriptAsync(x => x.ScriptChange, "Published");
                        }

                        await operationContext.ExecuteScriptAndTransformAsync(x => x.ScriptCreate, "Create");
                        await operationContext.EnrichAsync();
                        await operationContext.ValidateAsync();

                        Create(c);

                        return EntityCreatedResult.Create(c.Data, NewVersion);
                    });

                case UpdateContent updateContent:
                    return UpdateReturnAsync(updateContent, async c =>
                    {
                        GuardContent.CanUpdate(c);

                        var operationContext = await CreateContext(c, () => "Failed to update content.");

                        await operationContext.ValidateAsync();
                        await operationContext.ExecuteScriptAndTransformAsync(x => x.ScriptUpdate, "Update");

                        Update(c);

                        return new ContentDataChangedResult(Snapshot.Data, NewVersion);
                    });

                case PatchContent patchContent:
                    return UpdateReturnAsync(patchContent, async c =>
                    {
                        GuardContent.CanPatch(c);

                        var operationContext = await CreateContext(c, () => "Failed to patch content.");

                        await operationContext.ValidatePartialAsync();
                        await operationContext.ExecuteScriptAndTransformAsync(x => x.ScriptUpdate, "Patch");

                        Patch(c);

                        return new ContentDataChangedResult(Snapshot.Data, NewVersion);
                    });

                case ChangeContentStatus patchContent:
                    return UpdateAsync(patchContent, async c =>
                    {
                        GuardContent.CanChangeContentStatus(Snapshot.Status, c);

                        if (!c.DueTime.HasValue)
                        {
                            var operationContext = await CreateContext(c, () => "Failed to patch content.");

                            await operationContext.ExecuteScriptAsync(x => x.ScriptChange, c.Status);
                        }

                        ChangeStatus(c);
                    });

                case DeleteContent deleteContent:
                    return UpdateAsync(deleteContent, async c =>
                    {
                        GuardContent.CanDelete(c);

                        var operationContext = await CreateContext(c, () => "Failed to delete content.");

                        await operationContext.ExecuteScriptAsync(x => x.ScriptDelete, "Delete");

                        Delete(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        public void Create(CreateContent command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentCreated()));

            if (command.Publish)
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged { Status = Status.Published }));
            }
        }

        public void Update(UpdateContent command)
        {
            if (!command.Data.Equals(Snapshot.Data))
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentUpdated()));
            }
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

        public void Patch(PatchContent command)
        {
            var newData = command.Data.MergeInto(Snapshot.Data);

            if (!newData.Equals(Snapshot.Data))
            {
                var @event = SimpleMapper.Map(command, new ContentUpdated());

                @event.Data = newData;

                RaiseEvent(@event);
            }
        }

        public void Delete(DeleteContent command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentDeleted()));
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

        private async Task<ContentOperationContext> CreateContext(ContentCommand command, Func<string> message)
        {
            var operationContext =
                await ContentOperationContext.CreateAsync(command, Snapshot,
                    contentRepository,
                    appProvider,
                    assetRepository,
                    scriptEngine,
                    message);

            return operationContext;
        }
    }
}
