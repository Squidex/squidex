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

        public override Task<CommandResult> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateAsset c:
                    return CreateReturnAsync(c, async create =>
                    {
                        await GuardAsset.CanCreate(create, assetQuery);

                        if (create.Tags != null)
                        {
                            create.Tags = await NormalizeTagsAsync(create.AppId.Id, create.Tags);
                        }

                        Create(create);

                        return Snapshot;
                    });

                case AnnotateAsset c:
                    return UpdateReturnAsync(c, async annotate =>
                    {
                        GuardAsset.CanAnnotate(annotate);

                        if (annotate.Tags != null)
                        {
                            annotate.Tags = await NormalizeTagsAsync(Snapshot.AppId.Id, annotate.Tags);
                        }

                        Annotate(annotate);

                        return Snapshot;
                    });

                case UpdateAsset c:
                    return UpdateReturn(c, update =>
                    {
                        GuardAsset.CanUpdate(update);

                        Update(update);

                        return Snapshot;
                    });

                case MoveAsset c:
                    return UpdateReturnAsync(c, async move =>
                    {
                        await GuardAsset.CanMove(move, Snapshot, assetQuery);

                        Move(move);

                        return Snapshot;
                    });

                case DeleteAsset c:
                    return UpdateAsync(c, async delete =>
                    {
                        await GuardAsset.CanDelete(delete, Snapshot, contentRepository);

                        await NormalizeTagsAsync(Snapshot.AppId.Id, null);

                        Delete(delete);
                    });
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<HashSet<string>> NormalizeTagsAsync(DomainId appId, HashSet<string>? tags)
        {
            var normalized = await assetTags.NormalizeTagsAsync(appId, TagGroups.Assets, tags, Snapshot.Tags);

            return new HashSet<string>(normalized.Values);
        }

        private void Create(CreateAsset command)
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

        private void Update(UpdateAsset command)
        {
            Raise(command, new AssetUpdated
            {
                MimeType = command.File.MimeType,
                FileVersion = Snapshot.FileVersion + 1,
                FileSize = command.File.FileSize
            });
        }

        private void Annotate(AnnotateAsset command)
        {
            Raise(command, new AssetAnnotated());
        }

        private void Move(MoveAsset command)
        {
            Raise(command, new AssetMoved());
        }

        private void Delete(DeleteAsset command)
        {
            Raise(command, new AssetDeleted { DeletedSize = Snapshot.TotalSize });
        }

        private void Raise<T, TEvent>(T command, TEvent @event) where T : class where TEvent : AppEvent
        {
            RaiseEvent(Envelope.Create(SimpleMapper.Map(command, @event)));
        }
    }
}
