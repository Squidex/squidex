// ==========================================================================
//  ContentDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentDomainObject : DomainObjectBase
    {
        private bool isDeleted;
        private bool isCreated;
        private bool isPublished;
        private NamedContentData data;

        public bool IsDeleted
        {
            get { return isDeleted; }
        }

        public bool IsPublished
        {
            get { return isPublished; }
        }

        public ContentDomainObject(Guid id, int version)
            : base(id, version)
        {
        }

        protected void On(ContentCreated @event)
        {
            isCreated = true;

            data = @event.Data;
        }

        protected void On(ContentUpdated @event)
        {
            data = @event.Data;
        }

        protected void On(ContentPublished @event)
        {
            isPublished = true;
        }

        protected void On(ContentUnpublished @event)
        {
            isPublished = false;
        }

        protected void On(ContentDeleted @event)
        {
            isDeleted = true;
        }

        public ContentDomainObject Create(CreateContent command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create content");

            VerifyNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new ContentCreated()));

            if (command.Publish)
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentPublished()));
            }

            return this;
        }

        public ContentDomainObject Delete(DeleteContent command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new ContentDeleted()));

            return this;
        }

        public ContentDomainObject Publish(PublishContent command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new ContentPublished()));

            return this;
        }

        public ContentDomainObject Unpublish(UnpublishContent command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new ContentUnpublished()));

            return this;
        }

        public ContentDomainObject Update(UpdateContent command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot update content");

            VerifyCreatedAndNotDeleted();

            if (!command.Data.Equals(data))
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentUpdated()));
            }

            return this;
        }

        public ContentDomainObject Patch(PatchContent command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot patch content");

            VerifyCreatedAndNotDeleted();

            var newData = data.MergeInto(command.Data);

            if (!newData.Equals(data))
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentUpdated { Data = newData }));
            }

            return this;
        }

        private void VerifyNotCreated()
        {
            if (isCreated)
            {
                throw new DomainException("Content has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (isDeleted || !isCreated)
            {
                throw new DomainException("Content has already been deleted or not created yet.");
            }
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }
    }
}
