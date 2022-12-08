// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

#pragma warning disable MA0022 // Return Task.FromResult instead of returning null

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject;

public partial class RuleDomainObject : DomainObject<RuleDomainObject.State>
{
    private readonly IServiceProvider serviceProvider;

    public RuleDomainObject(DomainId id, IPersistenceFactory<State> persistence, ILogger<RuleDomainObject> log,
        IServiceProvider serviceProvider)
        : base(id, persistence, log)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override bool IsDeleted(State snapshot)
    {
        return snapshot.IsDeleted;
    }

    protected override bool CanAcceptCreation(ICommand command)
    {
        return command is RuleCommandBase;
    }

    protected override bool CanAccept(ICommand command)
    {
        return command is RuleCommand ruleCommand &&
            ruleCommand.AppId.Equals(Snapshot.AppId) &&
            ruleCommand.RuleId.Equals(Snapshot.Id);
    }

    public override Task<CommandResult> ExecuteAsync(IAggregateCommand command,
        CancellationToken ct)
    {
        switch (command)
        {
            case CreateRule createRule:
                return CreateReturnAsync(createRule, async (c, ct) =>
                {
                    await GuardRule.CanCreate(c, AppProvider());

                    Create(c);

                    return Snapshot;
                }, ct);

            case UpdateRule updateRule:
                return UpdateReturnAsync(updateRule, async (c, ct) =>
                {
                    await GuardRule.CanUpdate(c, Snapshot, AppProvider());

                    Update(c);

                    return Snapshot;
                }, ct);

            case EnableRule enable:
                return UpdateReturn(enable, c =>
                {
                    Enable(c);

                    return Snapshot;
                }, ct);

            case DisableRule disable:
                return UpdateReturn(disable, c =>
                {
                    Disable(c);

                    return Snapshot;
                }, ct);

            case DeleteRule delete:
                return Update(delete, c =>
                {
                    Delete(c);
                }, ct);

            case TriggerRule triggerRule:
                return UpdateReturnAsync(triggerRule, async (c, ct) =>
                {
                    await Trigger(triggerRule);

                    return true;
                }, ct);

            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    private async Task Trigger(TriggerRule command)
    {
        var @event = new RuleManuallyTriggered();

        SimpleMapper.Map(command, @event);
        SimpleMapper.Map(Snapshot, @event);

        await RuleEnqueuer().EnqueueAsync(Snapshot.RuleDef, Snapshot.Id, Envelope.Create(@event));
    }

    private IRuleEnqueuer RuleEnqueuer()
    {
        return serviceProvider.GetRequiredService<IRuleEnqueuer>();
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

    private IAppProvider AppProvider()
    {
        return serviceProvider.GetRequiredService<IAppProvider>();
    }
}
