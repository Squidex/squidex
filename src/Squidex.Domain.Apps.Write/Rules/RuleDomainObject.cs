// ==========================================================================
//  RuleDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Domain.Apps.Events.Rules.Utils;
using Squidex.Domain.Apps.Write.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Write.Rules
{
    public class RuleDomainObject : DomainObjectBase
    {
        private Rule rule;
        private bool isDeleted;

        public Rule Rule
        {
            get { return rule; }
        }

        public RuleDomainObject(Guid id, int version)
            : base(id, version)
        {
        }

        protected void On(RuleCreated @event)
        {
            rule = RuleEventDispatcher.Create(@event);
        }

        protected void On(RuleUpdated @event)
        {
            rule.Apply(@event);
        }

        protected void On(RuleEnabled @event)
        {
            rule.Apply(@event);
        }

        protected void On(RuleDisabled @event)
        {
            rule.Apply(@event);
        }

        protected void On(RuleDeleted @event)
        {
            isDeleted = true;
        }

        public void Create(CreateRule command)
        {
            VerifyNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new RuleCreated()));
        }

        public void Update(UpdateRule command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new RuleUpdated()));
        }

        public void Enable(EnableRule command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new RuleEnabled()));
        }

        public void Disable(DisableRule command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new RuleDisabled()));
        }

        public void Delete(DeleteRule command)
        {
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new RuleDeleted()));
        }

        private void VerifyNotCreated()
        {
            if (rule != null)
            {
                throw new DomainException("Webhook has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (isDeleted || rule == null)
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
