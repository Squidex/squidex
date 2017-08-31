// ==========================================================================
//  WebhookDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events.Webhooks;
using Squidex.Domain.Apps.Write.Webhooks.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Write.Webhooks
{
    public class WebhookDomainObject : DomainObjectBase
    {
        private bool isDeleted;
        private bool isCreated;

        public WebhookDomainObject(Guid id, int version)
            : base(id, version)
        {
        }

        protected void On(WebhookCreated @event)
        {
            isCreated = true;
        }

        protected void On(WebhookDeleted @event)
        {
            isDeleted = true;
        }

        public void Create(CreateWebhook command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create webhook");

            VerifyNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new WebhookCreated()));
        }

        public void Update(UpdateWebhook command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot update webhook");

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new WebhookUpdated()));
        }

        public void Delete(DeleteWebhook command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new WebhookDeleted()));
        }

        private void VerifyNotCreated()
        {
            if (isCreated)
            {
                throw new DomainException("Webhook has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (isDeleted || !isCreated)
            {
                throw new DomainException("Webhook has already been deleted or not created yet.");
            }
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }
    }
}
