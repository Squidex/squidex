// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Guards;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetGrain : SquidexDomainObjectGrainLogSnapshots<AssetState>, IAssetGrain
    {
        private readonly IAssetVerifier assetVerifier;

        public AssetGrain(IStore<Guid> store, ISemanticLog log, IAssetVerifier assetVerifier)
            : base(store, log)
        {
            Guard.NotNull(assetVerifier, nameof(assetVerifier));

            this.assetVerifier = assetVerifier;
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            VerifyNotDeleted();

            switch (command)
            {
                case CreateAsset createAsset:
                    return CreateReturnAsync(createAsset, async c =>
                    {
                        await GuardAsset.CanCreate(c, assetVerifier);

                        Create(c);

                        return new AssetSavedResult(Version, Snapshot.FileVersion);
                    });
                case CreateAssetFolder createAssetFolder:
                    return CreateReturn(createAssetFolder, c =>
                    {
                        GuardAsset.CanCreateFolder(c);

                        CreateFolder(c);

                        return new AssetSavedResult(Version, Snapshot.FileVersion);
                    });
                case UpdateAsset updateAsset:
                    return UpdateReturn(updateAsset, c =>
                    {
                        GuardAsset.CanUpdate(c, Snapshot.IsFolder);

                        Update(c);

                        return new AssetSavedResult(Version, Snapshot.FileVersion);
                    });
                case MoveAsset moveAsset:
                    return UpdateReturnAsync(moveAsset, async c =>
                    {
                        await GuardAsset.CanMove(c, assetVerifier, Snapshot.FolderId);

                        Move(c);

                        return new AssetSavedResult(Version, Snapshot.FileVersion);
                    });
                case RenameAsset renameAsset:
                    return Update(renameAsset, c =>
                    {
                        GuardAsset.CanRename(c, Snapshot.Name);

                        Rename(c);
                    });
                case DeleteAsset deleteAsset:
                    return Update(deleteAsset, c =>
                    {
                        GuardAsset.CanDelete(c);

                        Delete(c);
                    });
                default:
                    throw new NotSupportedException();
            }
        }

        public void Create(CreateAsset command)
        {
            var @event = SimpleMapper.Map(command, new AssetCreated
            {
                Name = command.File.FileName,
                FileSize = command.File.FileSize,
                FileVersion = 0,
                MimeType = command.File.MimeType,
                PixelWidth = command.ImageInfo?.PixelWidth,
                PixelHeight = command.ImageInfo?.PixelHeight,
                IsImage = command.ImageInfo != null
            });

            RaiseEvent(@event);
        }

        public void Update(UpdateAsset command)
        {
            VerifyNotDeleted();

            var @event = SimpleMapper.Map(command, new AssetUpdated
            {
                FileVersion = Snapshot.FileVersion + 1,
                FileSize = command.File.FileSize,
                MimeType = command.File.MimeType,
                PixelWidth = command.ImageInfo?.PixelWidth,
                PixelHeight = command.ImageInfo?.PixelHeight,
                IsImage = command.ImageInfo != null
            });

            RaiseEvent(@event);
        }

        public void CreateFolder(CreateAssetFolder command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AssetFolderCreated()));
        }

        public void Move(MoveAsset command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AssetMoved()));
        }

        public void Rename(RenameAsset command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AssetRenamed()));
        }

        public void Delete(DeleteAsset command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AssetDeleted { DeletedSize = Snapshot.TotalSize }));
        }

        private void RaiseEvent(AppEvent @event)
        {
            if (@event.AppId == null)
            {
                @event.AppId = Snapshot.AppId;
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private void VerifyNotDeleted()
        {
            if (Snapshot.IsDeleted)
            {
                throw new DomainException("Asset has already been deleted");
            }
        }

        protected override AssetState OnEvent(Envelope<IEvent> @event)
        {
            return Snapshot.Apply(@event);
        }

        public Task<J<IAssetEntity>> GetStateAsync(long version = EtagVersion.Any)
        {
            return J.AsTask<IAssetEntity>(GetSnapshot(version));
        }
    }
}
