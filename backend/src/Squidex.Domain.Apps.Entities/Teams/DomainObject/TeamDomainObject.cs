﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Domain.Apps.Entities.Teams.Commands.Guards;
using Squidex.Domain.Apps.Entities.Teams.DomainObject.Guards;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Shared.Users;

#pragma warning disable MA0022 // Return Task.FromResult instead of returning null

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject
{
    public partial class TeamDomainObject : DomainObject<TeamDomainObject.State>
    {
        private readonly IServiceProvider serviceProvider;

        public TeamDomainObject(DomainId id, IPersistenceFactory<State> persistence, ILogger<TeamDomainObject> log,
            IServiceProvider serviceProvider)
            : base(id, persistence, log)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override bool IsDeleted(State snapshot)
        {
            return false;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            return command is CreateTeam;
        }

        protected override bool CanAccept(ICommand command)
        {
            return command is TeamUpdateCommand update && Equals(update?.TeamId, Snapshot.Id);
        }

        public override Task<CommandResult> ExecuteAsync(IAggregateCommand command,
            CancellationToken ct)
        {
            switch (command)
            {
                case CreateTeam create:
                    return CreateReturn(create, c =>
                    {
                        GuardTeam.CanCreate(c);

                        Create(c);

                        return Snapshot;
                    }, ct);

                case UpdateTeam update:
                    return UpdateReturn(update, c =>
                    {
                        GuardTeam.CanUpdate(c);

                        Update(c);

                        return Snapshot;
                    }, ct);

                case AssignContributor assignContributor:
                    return UpdateReturnAsync(assignContributor, async (c, ct) =>
                    {
                        await GuardTeamContributors.CanAssign(c, Snapshot, Users());

                        AssignContributor(c, !Snapshot.Contributors.ContainsKey(assignContributor.ContributorId));

                        return Snapshot;
                    }, ct);

                case RemoveContributor removeContributor:
                    return UpdateReturn(removeContributor, c =>
                    {
                        GuardTeamContributors.CanRemove(c, Snapshot);

                        RemoveContributor(c);

                        return Snapshot;
                    }, ct);

                case ChangePlan changePlan:
                    return ChangeBillingPlanAsync(changePlan, ct);

                default:
                    ThrowHelper.NotSupportedException();
                    return default!;
            }
        }

        private async Task<CommandResult> ChangeBillingPlanAsync(ChangePlan changePlan,
            CancellationToken ct)
        {
            var userId = changePlan.Actor.Identifier;

            var result = await UpdateReturnAsync(changePlan, async (c, ct) =>
            {
                GuardTeam.CanChangePlan(c, Plans());

                if (string.Equals(GetFreePlan()?.Id, c.PlanId, StringComparison.Ordinal))
                {
                    ResetPlan(c);

                    return new PlanChangedResult(c.PlanId, true, null);
                }

                if (!c.FromCallback)
                {
                    var redirectUri = await Billing().MustRedirectToPortalAsync(userId, UniqueId, c.PlanId, ct);

                    if (redirectUri != null)
                    {
                        return new PlanChangedResult(c.PlanId, false, redirectUri);
                    }
                }

                ChangePlan(c);

                return new PlanChangedResult(c.PlanId);
            }, ct);

            if (changePlan.FromCallback)
            {
                return result;
            }

            if (result.Payload is PlanChangedResult { Unsubscribed: true, RedirectUri: null })
            {
                await Billing().UnsubscribeAsync(userId, UniqueId, default);
            }
            else if (result.Payload is PlanChangedResult { RedirectUri: null })
            {
                await Billing().SubscribeAsync(userId, UniqueId, changePlan.PlanId, default);
            }

            return result;
        }

        private void Create(CreateTeam command)
        {
            var events = new List<TeamEvent>
            {
                CreateInitalEvent(command.Name)
            };

            if (command.Actor.IsUser)
            {
                events.Add(CreateInitialOwner(command.Actor, command.Name));
            }

            foreach (var @event in events)
            {
                @event.TeamId = command.TeamId;

                Raise(command, @event);
            }
        }

        private void ChangePlan(ChangePlan command)
        {
            Raise(command, new TeamPlanChanged());
        }

        private void ResetPlan(ChangePlan command)
        {
            Raise(command, new TeamPlanReset());
        }

        private void Update(UpdateTeam command)
        {
            Raise(command, new TeamUpdated());
        }

        private void AssignContributor(AssignContributor command, bool isAdded)
        {
            Raise(command, new TeamContributorAssigned { IsAdded = isAdded, TeamName = Snapshot.Name });
        }

        private void RemoveContributor(RemoveContributor command)
        {
            Raise(command, new TeamContributorRemoved());
        }

        private void Raise<T, TEvent>(T command, TEvent @event) where T : class where TEvent : TeamEvent
        {
            SimpleMapper.Map(command, @event);

            @event.TeamId = Snapshot.Id;

            RaiseEvent(Envelope.Create(@event));
        }

        private static TeamCreated CreateInitalEvent(string name)
        {
            return new TeamCreated { Name = name };
        }

        private static TeamContributorAssigned CreateInitialOwner(RefToken actor, string name)
        {
            return new TeamContributorAssigned { ContributorId = actor.Identifier, Role = Role.Owner, TeamName = name };
        }

        private IBillingPlans Plans()
        {
            return serviceProvider.GetRequiredService<IBillingPlans>();
        }

        private IBillingManager Billing()
        {
            return serviceProvider.GetRequiredService<IBillingManager>();
        }

        private IUserResolver Users()
        {
            return serviceProvider.GetRequiredService<IUserResolver>();
        }

        private Plan GetFreePlan()
        {
            return Plans().GetFreePlan();
        }
    }
}
