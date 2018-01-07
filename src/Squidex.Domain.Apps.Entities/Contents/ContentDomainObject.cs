// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

            if (!command.Data.Equals(Snapshot.Data))
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentUpdated()));
            }

            return this;
        }

        public ContentDomainObject Patch(PatchContent command)
        {
            VerifyCreatedAndNotDeleted();

            var newData = command.Data.MergeInto(Snapshot.Data);

            if (!newData.Equals(Snapshot.Data))
            {
                var @event = SimpleMapper.Map(command, new ContentUpdated());

                @event.Data = newData;

                RaiseEvent(@event);
            }

            return this;
        }

        private void VerifyNotCreated()
        {
            if (Snapshot.Data != null)
            {
                throw new DomainException("Content has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (Snapshot.IsDeleted || Snapshot.Data == null)
            {
                throw new DomainException("Content has already been deleted or not created yet.");
            }
        }

        public override void ApplyEvent(Envelope<IEvent> @event)
        {
            ApplySnapshot(Snapshot.Apply(@event));
        }
    }
}
