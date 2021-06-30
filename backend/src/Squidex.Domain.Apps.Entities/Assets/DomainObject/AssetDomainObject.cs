// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
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
    public sealed partial class AssetDomainObject : DomainObject<AssetDomainObject.State>
    {
        private readonly IContentRepository contentRepository;
        private readonly IAssetTagService assetTags;
        private readonly IAssetQueryService assetQuery;

        public AssetDomainObject(IPersistenceFactory<AssetDomainObject.State> factory, ISemanticLog log,
            IAssetTagService assetTags,
            IAssetQueryService assetQuery,
            IContentRepository contentRepository)
            : base(factory, log)
        {
            this.assetTags = assetTags;
            this.assetQuery = assetQuery;
            this.contentRepository = contentRepository;

            Capacity = int.MaxValue;
        }

        protected override bool IsDeleted()
        {
            return Snapshot.IsDeleted;
        }

        protected override bool CanRecreate()
        {
            return true;
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
                case UpsertAsset upsert:
                    return UpsertReturnAsync(upsert, async c =>
                    {
                        if (Version > EtagVersion.Empty && !IsDeleted())
                        {
                            UpdateCore(c.AsUpdate());
                        }
                        else
                        {
                            await CreateCore(c.AsCreate());
                        }

                        if (Is.OptionalChange(Snapshot.ParentId, c.ParentId))
                        {
                            await MoveCore(c.AsMove(c.ParentId.Value));
                        }

                        return Snapshot;
                    });

                case CreateAsset c:
                    return CreateReturnAsync(c, async create =>
                    {
                        await CreateCore(create);

                        if (Is.Change(Snapshot.ParentId, c.ParentId))
                        {
                            await MoveCore(c.AsMove());
                        }

                        return Snapshot;
                    });

                case AnnotateAsset c:
                    return UpdateReturnAsync(c, async c =>
                    {
                        if (c.Tags != null)
                        {
                            c.Tags = await NormalizeTagsAsync(Snapshot.AppId.Id, c.Tags);
                        }

                        Annotate(c);

                        return Snapshot;
                    });

                case UpdateAsset update:
                    return UpdateReturn(update, update =>
                    {
                        Update(update);

                        return Snapshot;
                    });

                case MoveAsset move:
                    return UpdateReturnAsync(move, async c =>
                    {
                        await MoveCore(c);

                        return Snapshot;
                    });

                case DeleteAsset delete when delete.Permanent:
                    return DeletePermanentAsync(delete, async c =>
                    {
                        await DeleteCore(c);
                    });

                case DeleteAsset delete:
                    return UpdateAsync(delete, async c =>
                    {
                        await DeleteCore(c);
                    });
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task CreateCore(CreateAsset create)
        {
            if (create.Tags != null)
            {
                create.Tags = await NormalizeTagsAsync(create.AppId.Id, create.Tags);
            }

            Create(create);
        }

        private void UpdateCore(UpdateAsset update)
        {
            Update(update);
        }

        private async Task MoveCore(MoveAsset move)
        {
            await GuardAsset.CanMove(move, Snapshot, assetQuery);

            Move(move);
        }

        private async Task DeleteCore(DeleteAsset delete)
        {
            await GuardAsset.CanDelete(delete, Snapshot, contentRepository);

            await NormalizeTagsAsync(Snapshot.AppId.Id, null);

            Delete(delete);
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
