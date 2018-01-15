// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetDomainObject : SquidexDomainObjectBase<AssetState>
    {
        public AssetDomainObject Create(CreateAsset command)
        {
            VerifyNotCreated();

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

            return this;
        }

        public AssetDomainObject Update(UpdateAsset command)
        {
            VerifyCreatedAndNotDeleted();

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

            return this;
        }

        public AssetDomainObject Delete(DeleteAsset command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new AssetDeleted { DeletedSize = Snapshot.TotalSize }));

            return this;
        }

        public AssetDomainObject Rename(RenameAsset command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new AssetRenamed()));

            return this;
        }

        private void VerifyNotCreated()
        {
            if (!string.IsNullOrWhiteSpace(Snapshot.FileName))
            {
                throw new DomainException("Asset has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (Snapshot.IsDeleted || string.IsNullOrWhiteSpace(Snapshot.FileName))
            {
                throw new DomainException("Asset has already been deleted or not created yet.");
            }
        }

        public override void ApplyEvent(Envelope<IEvent> @event)
        {
            ApplySnapshot(Snapshot.Apply(@event));
        }
    }
}
