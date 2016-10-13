// ==========================================================================
//  AppDomainObject.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Events.Apps;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Write.Apps.Commands;

namespace PinkParrot.Write.Apps
{
    public sealed class AppDomainObject : DomainObject
    {
        private string name;

        public string Name
        {
            get { return name; }
        }

        public AppDomainObject(Guid id, int version) : base(id, version)
        {
        }

        public void On(AppCreated @event)
        {
            name = @event.Name;
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }

        public void Create(CreateApp command)
        {
            Guard.Valid(command, nameof(command), "Cannot create app");

            VerifyNotCreated();

            RaiseEvent(new AppCreated { Name = command.Name });
        }

        private void VerifyNotCreated()
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("App has already been created.");
            }
        }
    }
}
