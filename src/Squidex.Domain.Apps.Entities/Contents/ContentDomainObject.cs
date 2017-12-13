// ==========================================================================
//  ContentDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentDomainObject : DomainObjectBase<ContentState>
    {
        public ContentDomainObject Create(CreateContent command)
        {
            VerifyNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new ContentCreated()));

            if (command.Publish)
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged { Status = Status.Published }));
            }

            return this;
        }

        public ContentDomainObject Delete(DeleteContent command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new ContentDeleted()));

            return this;
        }

        public ContentDomainObject ChangeStatus(ChangeContentStatus command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged()));

            return this;
        }

        public ContentDomainObject Update(UpdateContent command)
        {
            VerifyCreatedAndNotDeleted();

            if (!command.Data.Equals(State.Data))
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentUpdated()));
            }

            return this;
        }

        public ContentDomainObject Patch(PatchContent command)
        {
            VerifyCreatedAndNotDeleted();

            var newData = command.Data.MergeInto(State.Data);

            if (!newData.Equals(State.Data))
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentUpdated { Data = newData }));
            }

            return this;
        }

        private void VerifyNotCreated()
        {
            if (State.Data != null)
            {
                throw new DomainException("Content has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (State.IsDeleted || State.Data == null)
            {
                throw new DomainException("Content has already been deleted or not created yet.");
            }
        }

        protected override void OnRaised(Envelope<IEvent> @event)
        {
            UpdateState(State.Apply(@event));
        }
    }
}
