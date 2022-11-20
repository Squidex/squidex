// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject;

public class RuleDomainObjectTests : HandlerTestBase<RuleDomainObject.State>
{
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly IRuleEnqueuer ruleEnqueuer = A.Fake<IRuleEnqueuer>();
    private readonly DomainId ruleId = DomainId.NewGuid();
    private readonly RuleDomainObject sut;

    protected override DomainId Id
    {
        get => DomainId.Combine(AppId, ruleId);
    }

    public sealed record TestAction : RuleAction
    {
        public int Value { get; set; }
    }

    public RuleDomainObjectTests()
    {
        var log = A.Fake<ILogger<RuleDomainObject>>();

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(appProvider)
                .AddSingleton(ruleEnqueuer)
                .BuildServiceProvider();

#pragma warning disable MA0056 // Do not call overridable members in constructor
        sut = new RuleDomainObject(Id, PersistenceFactory, log, serviceProvider);
#pragma warning restore MA0056 // Do not call overridable members in constructor
    }

    [Fact]
    public async Task Command_should_throw_exception_if_rule_is_deleted()
    {
        await ExecuteCreateAsync();
        await ExecuteDeleteAsync();

        await Assert.ThrowsAsync<DomainObjectDeletedException>(ExecuteDisableAsync);
    }

    [Fact]
    public async Task Create_should_create_events_and_set_intitial_state()
    {
        var command = MakeCreateCommand();

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(AppId, sut.Snapshot.AppId.Id);

        Assert.Same(command.Trigger, sut.Snapshot.RuleDef.Trigger);
        Assert.Same(command.Action, sut.Snapshot.RuleDef.Action);

        LastEvents
            .ShouldHaveSameEvents(
                CreateRuleEvent(new RuleCreated { Trigger = command.Trigger!, Action = command.Action! })
            );
    }

    [Fact]
    public async Task Update_should_create_events_and_update_trigger_and_action()
    {
        var command = MakeUpdateCommand();

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.True(sut.Snapshot.RuleDef.IsEnabled);

        Assert.Same(command.Trigger, sut.Snapshot.RuleDef.Trigger);
        Assert.Same(command.Action, sut.Snapshot.RuleDef.Action);

        Assert.Equal(command.Name, sut.Snapshot.RuleDef.Name);

        LastEvents
            .ShouldHaveSameEvents(
                CreateRuleEvent(new RuleUpdated { Trigger = command.Trigger, Action = command.Action, Name = command.Name })
            );
    }

    [Fact]
    public async Task Enable_should_create_events_and_update_enabled_flag()
    {
        var command = new EnableRule();

        await ExecuteCreateAsync();
        await ExecuteDisableAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.True(sut.Snapshot.RuleDef.IsEnabled);

        LastEvents
            .ShouldHaveSameEvents(
                CreateRuleEvent(new RuleEnabled())
            );
    }

    [Fact]
    public async Task Enable_via_update_should_create_events_and_update_enabled_flag()
    {
        var command = new UpdateRule
        {
            IsEnabled = true
        };

        await ExecuteCreateAsync();
        await ExecuteDisableAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.True(sut.Snapshot.RuleDef.IsEnabled);

        LastEvents
            .ShouldHaveSameEvents(
                CreateRuleEvent(new RuleUpdated { IsEnabled = true })
            );
    }

    [Fact]
    public async Task Disable_should_create_events_and_update_enabled_flag()
    {
        var command = new DisableRule();

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.False(sut.Snapshot.RuleDef.IsEnabled);

        LastEvents
            .ShouldHaveSameEvents(
                CreateRuleEvent(new RuleDisabled())
            );
    }

    [Fact]
    public async Task Disable_via_update_should_create_events_and_update_enabled_flag()
    {
        var command = new UpdateRule
        {
            IsEnabled = false
        };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.False(sut.Snapshot.RuleDef.IsEnabled);

        LastEvents
            .ShouldHaveSameEvents(
                CreateRuleEvent(new RuleUpdated { IsEnabled = false })
            );
    }

    [Fact]
    public async Task Delete_should_create_events_and_update_deleted_flag()
    {
        var command = new DeleteRule();

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(None.Value);

        Assert.True(sut.Snapshot.IsDeleted);

        LastEvents
            .ShouldHaveSameEvents(
                CreateRuleEvent(new RuleDeleted())
            );
    }

    [Fact]
    public async Task Trigger_should_invoke_rule_enqueue_but_not_change_snapshot()
    {
        var command = new TriggerRule();

        await ExecuteCreateAsync();

        await PublishAsync(command);

        Assert.Equal(0, sut.Version);

        A.CallTo(() => ruleEnqueuer.EnqueueAsync(sut.Snapshot.RuleDef, sut.Snapshot.Id,
                A<Envelope<IEvent>>.That.Matches(x => x.Payload is RuleManuallyTriggered)))
            .MustHaveHappened();
    }

    private Task ExecuteCreateAsync()
    {
        return PublishAsync(MakeCreateCommand());
    }

    private Task ExecuteDisableAsync()
    {
        return PublishAsync(new DisableRule());
    }

    private Task ExecuteDeleteAsync()
    {
        return PublishAsync(new DeleteRule());
    }

    private static CreateRule MakeCreateCommand()
    {
        return new CreateRule
        {
            Trigger = new ContentChangedTriggerV2
            {
                HandleAll = false
            },
            Action = new TestAction
            {
                Value = 123
            }
        };
    }

    private static UpdateRule MakeUpdateCommand()
    {
        return new UpdateRule
        {
            Name = "NewName",
            Trigger = new ContentChangedTriggerV2
            {
                HandleAll = true
            },
            Action = new TestAction
            {
                Value = 456
            }
        };
    }

    private T CreateRuleEvent<T>(T @event) where T : RuleEvent
    {
        @event.RuleId = ruleId;

        return CreateEvent(@event);
    }

    private T CreateRuleCommand<T>(T command) where T : RuleCommand
    {
        command.RuleId = ruleId;

        return CreateCommand(command);
    }

    private Task<object> PublishIdempotentAsync(RuleCommand command)
    {
        return PublishIdempotentAsync(sut, CreateRuleCommand(command));
    }

    private async Task<object?> PublishAsync(RuleCommand command)
    {
        var actual = await sut.ExecuteAsync(CreateRuleCommand(command), default);

        return actual.Payload;
    }
}
