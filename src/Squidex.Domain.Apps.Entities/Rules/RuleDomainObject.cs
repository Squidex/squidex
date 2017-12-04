// ==========================================================================
//  RuleDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleDomainObject : DomainObjectBase<RuleDomainObject, RuleState>
    {
        public void Create(CreateRule command)
        {
            VerifyNotCreated();

            UpdateRule(command, r => new Rule(command.Trigger, command.Action));

            RaiseEvent(SimpleMapper.Map(command, new RuleCreated()));
        }

        public void Update(UpdateRule command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateRule(command, r => r.Update(command.Trigger).Update(command.Action));

            RaiseEvent(SimpleMapper.Map(command, new RuleUpdated()));
        }

        public void Enable(EnableRule command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateRule(command, r => r.Enable());

            RaiseEvent(SimpleMapper.Map(command, new RuleEnabled()));
        }

        public void Disable(DisableRule command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateRule(command, r => r.Disable());

            RaiseEvent(SimpleMapper.Map(command, new RuleDisabled()));
        }

        public void Delete(DeleteRule command)
        {
            VerifyCreatedAndNotDeleted();

            UpdateState(command, s => s.IsDeleted = true);

            RaiseEvent(SimpleMapper.Map(command, new RuleDeleted()));
        }

        private void VerifyNotCreated()
        {
            if (State.RuleDef != null)
            {
                throw new DomainException("Webhook has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (State.IsDeleted || State.RuleDef == null)
            {
                throw new DomainException("Webhook has already been deleted or not created yet.");
            }
        }

        private void UpdateRule(ICommand command, Func<Rule, Rule> updater)
        {
            UpdateState(command, s => s.RuleDef = updater(s.RuleDef));
        }

        protected override RuleState CloneState(ICommand command, Action<RuleState> updater)
        {
            return State.Clone().Update((SquidexCommand)command, updater);
        }
    }
}
