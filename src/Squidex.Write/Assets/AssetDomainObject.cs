// ==========================================================================
//  AssetDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Write.Assets.Commands;

// ReSharper disable UnusedParameter.Local

namespace Squidex.Write.Assets
{
    public class AssetDomainObject : DomainObjectBase
    {
        private bool isDeleted;
        private string fileName;

        public bool IsDeleted
        {
            get { return isDeleted; }
        }

        public string FileName
        {
            get { return fileName; }
        }

        public AssetDomainObject(Guid id, int version) 
            : base(id, version)
        {
        }

        protected void On(AssetCreated @event)
        {
            fileName = @event.FileName;
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

            RaiseEvent(SimpleMapper.Map(command, new AssetCreated()));

            return this;
        }

        public AssetDomainObject Delete(DeleteAsset command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new AssetDeleted()));

            return this;
        }

        public AssetDomainObject Update(UpdateAsset command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new AssetUpdated()));

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
