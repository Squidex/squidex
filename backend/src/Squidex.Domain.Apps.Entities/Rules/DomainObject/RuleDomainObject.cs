// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject
{
    public sealed partial class RuleDomainObject : DomainObject<RuleDomainObject.State>
    {
        private readonly IAppProvider appProvider;
        private readonly IRuleEnqueuer ruleEnqueuer;

        public RuleDomainObject(IPersistenceFactory<State> factory, ISemanticLog log,
            IAppProvider appProvider, IRuleEnqueuer ruleEnqueuer)
            : base(factory, log)
        {
            this.appProvider = appProvider;
            this.ruleEnqueuer = ruleEnqueuer;
        }

        protected override bool IsDeleted()
        {
            return Snapshot.IsDeleted;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            return command is RuleCommand;
        }

        protected override bool CanAccept(ICommand command)
        {
            return command is RuleCommand ruleCommand &&
                ruleCommand.AppId.Equals(Snapshot.AppId) &&
                ruleCommand.RuleId.Equals(Snapshot.Id);
        }

        public override Task<CommandResult> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateRule createRule:
                    return CreateReturnAsync(createRule, async c =>
                    {
                        await GuardRule.CanCreate(c, appProvider);

                        Create(c);

                        return Snapshot;
                    });

                case UpdateRule updateRule:
                    return UpdateReturnAsync(updateRule, async c =>
                    {
                        await GuardRule.CanUpdate(c, Snapshot, appProvider);

                        Update(c);

                        return Snapshot;
                    });

                case EnableRule enable:
                    return UpdateReturn(enable, c =>
                    {
                        Enable(c);

                        return Snapshot;
                    });

                case DisableRule disable:
                    return UpdateReturn(disable, c =>
                    {
                        Disable(c);

                        return Snapshot;
                    });

                case DeleteRule delete:
                    return Update(delete, c =>
                    {
                        Delete(c);
                    });

                case TriggerRule triggerRule:
                    return UpdateReturnAsync(triggerRule, async c =>
                    {
                        await Trigger(triggerRule);

                        return true;
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task Trigger(TriggerRule command)
        {
            var @event = new RuleManuallyTriggered();

            SimpleMapper.Map(command, @event);
            SimpleMapper.Map(Snapshot, @event);

            await ruleEnqueuer.EnqueueAsync(Snapshot.RuleDef, Snapshot.Id, Envelope.Create(@event));
        }

        private void Create(CreateRule command)
        {
            Raise(command, new RuleCreated());
        }

        private void Update(UpdateRule command)
        {
            Raise(command, new RuleUpdated());
        }

        private void Enable(EnableRule command)
        {
            Raise(command, new RuleEnabled());
        }

        private void Disable(DisableRule command)
        {
            Raise(command, new RuleDisabled());
        }

        private void Delete(DeleteRule command)
        {
            Raise(command, new RuleDeleted());
        }

        private void Raise<T, TEvent>(T command, TEvent @event) where T : class where TEvent : AppEvent
        {
            RaiseEvent(Envelope.Create(SimpleMapper.Map(command, @event)));
        }
    }
}
