// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.Guards;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleDomainObject : DomainObject<RuleState>
    {
        private readonly IAppProvider appProvider;
        private readonly IRuleEnqueuer ruleEnqueuer;

        public RuleDomainObject(IStore<DomainId> store, ISemanticLog log,
            IAppProvider appProvider, IRuleEnqueuer ruleEnqueuer)
            : base(store, log)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(ruleEnqueuer, nameof(ruleEnqueuer));

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

        public override Task<object?> ExecuteAsync(IAggregateCommand command)
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
                        await GuardRule.CanUpdate(c, Snapshot.AppId.Id, appProvider);

                        Update(c);

                        return Snapshot;
                    });
                case EnableRule enableRule:
                    return UpdateReturn(enableRule, c =>
                    {
                        GuardRule.CanEnable(c);

                        Enable(c);

                        return Snapshot;
                    });
                case DisableRule disableRule:
                    return UpdateReturn(disableRule, c =>
                    {
                        GuardRule.CanDisable(c);

                        Disable(c);

                        return Snapshot;
                    });
                case DeleteRule deleteRule:
                    return Update(deleteRule, c =>
                    {
                        GuardRule.CanDelete(deleteRule);

                        Delete(c);
                    });
                case TriggerRule triggerRule:
                    return Trigger(triggerRule);
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<object?> Trigger(TriggerRule command)
        {
            await EnsureLoadedAsync();

            var @event = SimpleMapper.Map(command, new RuleManuallyTriggered { RuleId = Snapshot.Id, AppId = Snapshot.AppId });

            await ruleEnqueuer.EnqueueAsync(Snapshot.RuleDef, Snapshot.Id, Envelope.Create(@event));

            return null;
        }

        public void Create(CreateRule command)
        {
            RaiseEvent(SimpleMapper.Map(command, new RuleCreated()));
        }

        public void Update(UpdateRule command)
        {
            RaiseEvent(SimpleMapper.Map(command, new RuleUpdated()));
        }

        public void Enable(EnableRule command)
        {
            RaiseEvent(SimpleMapper.Map(command, new RuleEnabled()));
        }

        public void Disable(DisableRule command)
        {
            RaiseEvent(SimpleMapper.Map(command, new RuleDisabled()));
        }

        public void Delete(DeleteRule command)
        {
            RaiseEvent(SimpleMapper.Map(command, new RuleDeleted()));
        }

        private void RaiseEvent(AppEvent @event)
        {
            @event.AppId ??= Snapshot.AppId;

            RaiseEvent(Envelope.Create(@event));
        }
    }
}
