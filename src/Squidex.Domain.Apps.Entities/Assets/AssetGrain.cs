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
    public class AssetGrain : SquidexDomainObjectGrain<AssetState>, IAssetGrain
    {
        public AssetGrain(IStore<Guid> store, ISemanticLog log)
            : base(store, log)
        {
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateAsset createRule:
                    return CreateReturnAsync(createRule, c =>
                    {
                        GuardAsset.CanCreate(c);

                        Create(c);

                        return new AssetSavedResult(NewVersion, Snapshot.FileVersion);
                    });
                case UpdateAsset updateRule:
                    return UpdateReturnAsync(updateRule, c =>
                    {
                        GuardAsset.CanUpdate(c);

                        Update(c);

                        return new AssetSavedResult(NewVersion, Snapshot.FileVersion);
                    });
                case RenameAsset renameAsset:
                    return UpdateAsync(renameAsset, c =>
                    {
                        GuardAsset.CanRename(c, Snapshot.FileName);

                        Rename(c);
                    });
                case DeleteAsset deleteAsset:
                    return UpdateAsync(deleteAsset, c =>
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
                FileName = command.File.FileName,
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

        public void Delete(DeleteAsset command)
        {
            VerifyNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new AssetDeleted { DeletedSize = Snapshot.TotalSize }));
        }

        public void Rename(RenameAsset command)
        {
            VerifyNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new AssetRenamed()));
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

        public override void ApplyEvent(Envelope<IEvent> @event)
        {
            ApplySnapshot(Snapshot.Apply(@event));
        }

        public Task<J<IAssetEntity>> GetStateAsync()
        {
            return J.AsTask<IAssetEntity>(Snapshot);
        }
    }
}
