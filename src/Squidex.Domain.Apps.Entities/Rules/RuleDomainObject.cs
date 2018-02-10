// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class RuleDomainObject : DomainObjectBase<RuleState>
    {
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

        private void RaiseEvent(AppEvent @event)
        {
            if (@event.AppId == null)
            {
                @event.AppId = Snapshot.AppId;
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private void VerifyNotCreated()
        {
            if (Snapshot.RuleDef != null)
            {
                throw new DomainException("Webhook has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (Snapshot.IsDeleted || Snapshot.RuleDef == null)
            {
                throw new DomainException("Webhook has already been deleted or not created yet.");
            }
        }

        public override void ApplyEvent(Envelope<IEvent> @event)
        {
            ApplySnapshot(Snapshot.Apply(@event));
        }
    }
}
