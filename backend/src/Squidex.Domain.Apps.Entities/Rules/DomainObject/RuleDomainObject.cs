// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Rules;
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

public partial class RuleDomainObject : DomainObject<Rule>
{
    private readonly IServiceProvider serviceProvider;

    public RuleDomainObject(DomainId id, IPersistenceFactory<Rule> persistence, ILogger<RuleDomainObject> log,
        IServiceProvider serviceProvider)
        : base(id, persistence, log)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override bool IsDeleted(Rule snapshot)
    {
        return snapshot.IsDeleted;
    }

    protected override bool CanAccept(ICommand command)
    {
        return command is RuleCommand c && c.AppId.Equals(Snapshot.AppId) && c.RuleId.Equals(Snapshot.Id);
    }

    protected override bool CanAccept(ICommand command, DomainObjectState state)
    {
        switch (state)
        {
            case DomainObjectState.Undefined:
                return command is CreateRule;
            case DomainObjectState.Empty:
                return command is CreateRule;
            case DomainObjectState.Created:
                return command is not CreateRule;
            default:
                return false;
        }
    }

    public override Task<CommandResult> ExecuteAsync(IAggregateCommand command,
        CancellationToken ct)
    {
        switch (command)
        {
            case CreateRule createRule:
                return ApplyReturnAsync(createRule, async (c, ct) =>
                {
                    await GuardRule.CanCreate(c, AppProvider());

                    Create(c);

                    return Snapshot;
                }, ct);

            case UpdateRule updateRule:
                return ApplyReturnAsync(updateRule, async (c, ct) =>
                {
                    await GuardRule.CanUpdate(c, Snapshot, AppProvider());

                    Update(c);

                    return Snapshot;
                }, ct);

            case EnableRule enable:
                return ApplyReturn(enable, c =>
                {
                    Enable(c);

                    return Snapshot;
                }, ct);

            case DisableRule disable:
                return ApplyReturn(disable, c =>
                {
                    Disable(c);

                    return Snapshot;
                }, ct);

            case DeleteRule delete:
                return Apply(delete, c =>
                {
                    Delete(c);
                }, ct);

            case TriggerRule triggerRule:
                return ApplyReturnAsync(triggerRule, async (c, ct) =>
                {
                    await Trigger(triggerRule);

                    return None.Value;
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

        await RuleEnqueuer().EnqueueAsync(Snapshot.Id, Snapshot, Envelope.Create(@event));
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
