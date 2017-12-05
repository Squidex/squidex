// ==========================================================================
//  AssetDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetDomainObject : DomainObjectBase<AssetDomainObject, AssetState>
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

            UpdateState(command, s =>
            {
                s.TotalSize = @event.FileSize;

                SimpleMapper.Map(@event, s);
            });

            RaiseEvent(@event);

            return this;
        }

        public AssetDomainObject Update(UpdateAsset command)
        {
            VerifyCreatedAndNotDeleted();

            var @event = SimpleMapper.Map(command, new AssetUpdated
            {
                FileVersion = State.FileVersion + 1,
                FileSize = command.File.FileSize,
                MimeType = command.File.MimeType,
                PixelWidth = command.ImageInfo?.PixelWidth,
                PixelHeight = command.ImageInfo?.PixelHeight,
                IsImage = command.ImageInfo != null
            });

            UpdateState(command, s =>
            {
                s.TotalSize += @event.FileSize;

                SimpleMapper.Map(@event, s);
            });

            RaiseEvent(@event);

            return this;
        }

        public AssetDomainObject Delete(DeleteAsset command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateState(command, s => s.IsDeleted = true);

            RaiseEvent(SimpleMapper.Map(command, new AssetDeleted { DeletedSize = State.TotalSize }));

            return this;
        }

        public AssetDomainObject Rename(RenameAsset command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateState(command, s => s.FileName = command.FileName);

            RaiseEvent(SimpleMapper.Map(command, new AssetRenamed()));

            return this;
        }

        private void VerifyNotCreated()
        {
            if (!string.IsNullOrWhiteSpace(State.FileName))
            {
                throw new DomainException("Asset has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (State.IsDeleted || string.IsNullOrWhiteSpace(State.FileName))
            {
                throw new DomainException("Asset has already been deleted or not created yet.");
            }
        }

        protected override AssetState CloneState(ICommand command, Action<AssetState> updater)
        {
            return State.Clone().Update((SquidexCommand)command, updater);
        }
    }
}
