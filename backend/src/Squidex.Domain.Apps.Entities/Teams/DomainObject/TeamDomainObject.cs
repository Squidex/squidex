// ==========================================================================
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
using Squidex.Domain.Apps.Entities.Teams.DomainObject.Guards;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Shared.Users;

#pragma warning disable MA0022 // Return Task.FromResult instead of returning null

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject;

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
        return command is TeamCommandBase;
    }

    protected override bool CanAccept(ICommand command)
    {
        return command is TeamCommand update && Equals(update?.TeamId, Snapshot.Id);
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
                    await GuardTeamContributors.CanAssign(c, Snapshot, Users);

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
                return UpdateReturnAsync(changePlan, async (c, ct) =>
                {
                    GuardTeam.CanChangePlan(c, BillingPlans);

                    if (string.Equals(FreePlan?.Id, c.PlanId, StringComparison.Ordinal))
                    {
                        if (!c.FromCallback)
                        {
                            await BillingManager.UnsubscribeAsync(c.Actor.Identifier, Snapshot, default);
                        }

                        ResetPlan(c);

                        return new PlanChangedResult(c.PlanId, true, null);
                    }
                    else
                    {
                        if (!c.FromCallback)
                        {
                            var redirectUri = await BillingManager.MustRedirectToPortalAsync(c.Actor.Identifier, Snapshot, c.PlanId, ct);

                            if (redirectUri != null)
                            {
                                return new PlanChangedResult(c.PlanId, false, redirectUri);
                            }

                            await BillingManager.SubscribeAsync(c.Actor.Identifier, Snapshot, changePlan.PlanId, default);
                        }

                        ChangePlan(c);

                        return new PlanChangedResult(c.PlanId);
                    }
                }, ct);

            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    private void Create(CreateTeam command)
    {
        void RaiseInitial<T>(T @event) where T : TeamEvent
        {
            Raise(command, @event, command.TeamId);
        }

        RaiseInitial(new TeamCreated());

        var actor = command.Actor;

        if (actor.IsUser)
        {
            RaiseInitial(new TeamContributorAssigned { ContributorId = actor.Identifier, Role = Role.Owner });
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
        Raise(command, new TeamContributorAssigned { IsAdded = isAdded });
    }

    private void RemoveContributor(RemoveContributor command)
    {
        Raise(command, new TeamContributorRemoved());
    }

    private void Raise<T, TEvent>(T command, TEvent @event, DomainId? id = null) where T : class where TEvent : TeamEvent
    {
        SimpleMapper.Map(command, @event);

        @event.TeamId = id ?? Snapshot.Id;

        RaiseEvent(Envelope.Create(@event));
    }

    private IBillingPlans BillingPlans
    {
        get => serviceProvider.GetRequiredService<IBillingPlans>();
    }

    private IBillingManager BillingManager
    {
        get => serviceProvider.GetRequiredService<IBillingManager>();
    }

    private IUserResolver Users
    {
        get => serviceProvider.GetRequiredService<IUserResolver>();
    }

    private Plan FreePlan
    {
        get => BillingPlans.GetFreePlan();
    }
}
