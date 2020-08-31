// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Guards;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetDomainObject : LogSnapshotDomainObject<AssetState>
    {
        private readonly ITagService tagService;
        private readonly IAssetQueryService assetQuery;

        public AssetDomainObject(IStore<DomainId> store, ITagService tagService, IAssetQueryService assetQuery, ISemanticLog log)
            : base(store, log)
        {
            Guard.NotNull(tagService, nameof(tagService));
            Guard.NotNull(assetQuery, nameof(assetQuery));

            this.tagService = tagService;

            this.assetQuery = assetQuery;
        }

        protected override bool IsDeleted()
        {
            return Snapshot.IsDeleted;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            return command is AssetCommand;
        }

        protected override bool CanAccept(ICommand command)
        {
            return command is AssetCommand assetCommand &&
                Equals(assetCommand.AppId, Snapshot.AppId) &&
                Equals(assetCommand.AssetId, Snapshot.Id);
        }

        public override Task<object?> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateAsset createAsset:
                    return CreateReturnAsync(createAsset, async c =>
                    {
                        await GuardAsset.CanCreate(c, assetQuery);

                        var tagIds = await NormalizeTagsAsync(c.AppId.Id, c.Tags);

                        Create(c, tagIds);

                        return Snapshot;
                    });
                case UpdateAsset updateAsset:
                    return UpdateReturn(updateAsset, c =>
                    {
                        GuardAsset.CanUpdate(c);

                        Update(c);

                        return Snapshot;
                    });
                case AnnotateAsset annotateAsset:
                    return UpdateReturnAsync(annotateAsset, async c =>
                    {
                        GuardAsset.CanAnnotate(c);

                        var tagIds = await NormalizeTagsAsync(Snapshot.AppId.Id, c.Tags);

                        Annotate(c, tagIds);

                        return Snapshot;
                    });
                case MoveAsset moveAsset:
                    return UpdateReturnAsync(moveAsset, async c =>
                    {
                        await GuardAsset.CanMove(c, assetQuery, Snapshot.ParentId);

                        Move(c);

                        return Snapshot;
                    });
                case DeleteAsset deleteAsset:
                    return UpdateAsync(deleteAsset, async c =>
                    {
                        GuardAsset.CanDelete(c);

                        await tagService.NormalizeTagsAsync(Snapshot.AppId.Id, TagGroups.Assets, null, Snapshot.Tags);

                        Delete(c);
                    });
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<HashSet<string>?> NormalizeTagsAsync(DomainId appId, HashSet<string> tags)
        {
            if (tags == null)
            {
                return null;
            }

            var normalized = await tagService.NormalizeTagsAsync(appId, TagGroups.Assets, tags, Snapshot.Tags);

            return new HashSet<string>(normalized.Values);
        }

        public void Create(CreateAsset command, HashSet<string>? tagIds)
        {
            var @event = SimpleMapper.Map(command, new AssetCreated
            {
                FileName = command.File.FileName,
                FileSize = command.File.FileSize,
                FileVersion = 0,
                MimeType = command.File.MimeType,
                Slug = command.File.FileName.ToAssetSlug()
            });

            @event.Tags = tagIds;

            RaiseEvent(@event);
        }

        public void Update(UpdateAsset command)
        {
            var @event = SimpleMapper.Map(command, new AssetUpdated
            {
                FileVersion = Snapshot.FileVersion + 1,
                FileSize = command.File.FileSize,
                MimeType = command.File.MimeType
            });

            RaiseEvent(@event);
        }

        public void Annotate(AnnotateAsset command, HashSet<string>? tagIds)
        {
            var @event = SimpleMapper.Map(command, new AssetAnnotated());

            @event.Tags = tagIds;

            RaiseEvent(@event);
        }

        public void Move(MoveAsset command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AssetMoved()));
        }

        public void Delete(DeleteAsset command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AssetDeleted { DeletedSize = Snapshot.TotalSize }));
        }

        private void RaiseEvent(AppEvent @event)
        {
            @event.AppId ??= Snapshot.AppId;

            RaiseEvent(Envelope.Create(@event));
        }
    }
}
