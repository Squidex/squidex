// ==========================================================================
//  AppDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Write.Apps.Commands;

namespace Squidex.Write.Apps
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
            Guard.Valid(command, nameof(command), () => "Cannot create app");

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
