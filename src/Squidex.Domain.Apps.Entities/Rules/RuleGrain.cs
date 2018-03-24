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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class RuleGrain : SquidexDomainObjectGrain<RuleState>, IRuleGrain
    {
        private readonly IAppProvider appProvider;

        public RuleGrain(IStore<Guid> store, IAppProvider appProvider)
            : base(store)
        {
            Guard.NotNull(appProvider, nameof(appProvider));

            this.appProvider = appProvider;
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            VerifyNotDeleted();

            switch (command)
            {
                case CreateRule createRule:
                    return CreateAsync(createRule, async c =>
                    {
                        await GuardRule.CanCreate(c, appProvider);

                        Create(c);
                    });
                case UpdateRule updateRule:
                    return UpdateAsync(updateRule, async c =>
                    {
                        await GuardRule.CanUpdate(c, Snapshot.AppId.Id, appProvider);

                        Update(c);
                    });
                case EnableRule enableRule:
                    return UpdateAsync(enableRule, c =>
                    {
                        GuardRule.CanEnable(c, Snapshot.RuleDef);

                        Enable(c);
                    });
                case DisableRule disableRule:
                    return UpdateAsync(disableRule, c =>
                    {
                        GuardRule.CanDisable(c, Snapshot.RuleDef);

                        Disable(c);
                    });
                case DeleteRule deleteRule:
                    return UpdateAsync(deleteRule, c =>
                    {
                        GuardRule.CanDelete(deleteRule);

                        Delete(c);
                    });
                default:
                    throw new NotSupportedException();
            }
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
            if (@event.AppId == null)
            {
                @event.AppId = Snapshot.AppId;
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private void VerifyNotDeleted()
        {
            if (Snapshot.IsDeleted)
            {
                throw new DomainException("Webhook has already been deleted.");
            }
        }

        public override void ApplyEvent(Envelope<IEvent> @event)
        {
            ApplySnapshot(Snapshot.Apply(@event));
        }

        public Task<J<IRuleEntity>> GetStateAsync()
        {
            return Task.FromResult(new J<IRuleEntity>(Snapshot));
        }
    }
}
