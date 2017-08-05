// ==========================================================================
//  AssetDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Write.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

// ReSharper disable UnusedParameter.Local

namespace Squidex.Domain.Apps.Write.Assets
{
    public class AssetDomainObject : DomainObjectBase
    {
        private bool isDeleted;
        private long fileVersion = -1;
        private long totalSize;
        private string fileName;

        public bool IsDeleted
        {
            get { return isDeleted; }
        }

        public long FileVersion
        {
            get { return fileVersion; }
        }

        public AssetDomainObject(Guid id, int version)
            : base(id, version)
        {
        }

        protected void On(AssetCreated @event)
        {
            fileVersion = @event.FileVersion;
            fileName = @event.FileName;

            totalSize += @event.FileSize;
        }

        protected void On(AssetUpdated @event)
        {
            fileVersion = @event.FileVersion;

            totalSize += @event.FileSize;
        }

        protected void On(AssetRenamed @event)
        {
            fileName = @event.FileName;
        }

        protected void On(AssetDeleted @event)
        {
            isDeleted = true;
        }

        public AssetDomainObject Create(CreateAsset command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyNotCreated();

            var @event = SimpleMapper.Map(command, new AssetCreated
            {
                FileName = command.File.FileName,
                FileSize = command.File.FileSize,
                FileVersion = fileVersion + 1,
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
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            var @event = SimpleMapper.Map(command, new AssetUpdated
            {
                FileVersion = fileVersion + 1,
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
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new AssetDeleted { DeletedSize = totalSize }));

            return this;
        }

        public AssetDomainObject Rename(RenameAsset command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot rename asset.");

            VerifyCreatedAndNotDeleted();
            VerifyDifferentNames(command.FileName, () => "Cannot rename asset.");

            RaiseEvent(SimpleMapper.Map(command, new AssetRenamed()));

            return this;
        }

        private void VerifyDifferentNames(string newName, Func<string> message)
        {
            if (string.Equals(fileName, newName))
            {
                throw new ValidationException(message(), new ValidationError("The asset already has this name.", "Name"));
            }
        }

        private void VerifyNotCreated()
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                throw new DomainException("Asset has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (isDeleted || string.IsNullOrWhiteSpace(fileName))
            {
                throw new DomainException("Asset has already been deleted or not created yet.");
            }
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }
    }
}
