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
using Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;
using IAssetTagService = Squidex.Domain.Apps.Core.Tags.ITagService;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed partial class AssetDomainObject : LogSnapshotDomainObject<AssetDomainObject.State>
    {
        private readonly IContentRepository contentRepository;
        private readonly IAssetTagService assetTags;
        private readonly IAssetQueryService assetQuery;

        public AssetDomainObject(IStore<DomainId> store, ISemanticLog log,
            IAssetTagService assetTags,
            IAssetQueryService assetQuery,
            IContentRepository contentRepository)
            : base(store, log)
        {
            Guard.NotNull(assetTags, nameof(assetTags));
            Guard.NotNull(assetQuery, nameof(assetQuery));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.assetTags = assetTags;
            this.assetQuery = assetQuery;
            this.contentRepository = contentRepository;
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

                        if (c.Tags != null)
                        {
                            c.Tags = await NormalizeTagsAsync(c.AppId.Id, c.Tags);
                        }

                        Create(c);

                        return Snapshot;
                    });

                case AnnotateAsset annotateAsset:
                    return UpdateReturnAsync(annotateAsset, async c =>
                    {
                        GuardAsset.CanAnnotate(c);

                        if (c.Tags != null)
                        {
                            c.Tags = await NormalizeTagsAsync(Snapshot.AppId.Id, c.Tags);
                        }

                        Annotate(c);

                        return Snapshot;
                    });

                case UpdateAsset updateAsset:
                    return UpdateReturn(updateAsset, c =>
                    {
                        GuardAsset.CanUpdate(c);

                        Update(c);

                        return Snapshot;
                    });

                case MoveAsset moveAsset:
                    return UpdateReturnAsync(moveAsset, async c =>
                    {
                        await GuardAsset.CanMove(c, Snapshot, assetQuery);

                        Move(c);

                        return Snapshot;
                    });

                case DeleteAsset deleteAsset:
                    return UpdateAsync(deleteAsset, async c =>
                    {
                        await GuardAsset.CanDelete(c, Snapshot, contentRepository);

                        await assetTags.NormalizeTagsAsync(Snapshot.AppId.Id, TagGroups.Assets, null, Snapshot.Tags);

                        Delete(c);
                    });
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<HashSet<string>> NormalizeTagsAsync(DomainId appId, HashSet<string> tags)
        {
            var normalized = await assetTags.NormalizeTagsAsync(appId, TagGroups.Assets, tags, Snapshot.Tags);

            return new HashSet<string>(normalized.Values);
        }

        public void Create(CreateAsset command)
        {
            Raise(command, new AssetCreated
            {
                MimeType = command.File.MimeType,
                FileName = command.File.FileName,
                FileSize = command.File.FileSize,
                FileVersion = 0,
                Slug = command.File.FileName.ToAssetSlug()
            });
        }

        public void Update(UpdateAsset command)
        {
            Raise(command, new AssetUpdated
            {
                MimeType = command.File.MimeType,
                FileVersion = Snapshot.FileVersion + 1,
                FileSize = command.File.FileSize
            });
        }

        public void Annotate(AnnotateAsset command)
        {
            Raise(command, new AssetAnnotated());
        }

        public void Move(MoveAsset command)
        {
            Raise(command, new AssetMoved());
        }

        public void Delete(DeleteAsset command)
        {
            Raise(command, new AssetDeleted { DeletedSize = Snapshot.TotalSize });
        }

        private void Raise<T, TEvent>(T command, TEvent @event) where T : class where TEvent : AppEvent
        {
            SimpleMapper.Map(command, @event);

            @event.AppId ??= Snapshot.AppId;

            RaiseEvent(Envelope.Create(@event));
        }
    }
}
