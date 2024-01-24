// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject;

public class RuleDomainObjectTests : HandlerTestBase<Rule>
{
    private readonly IRuleEnqueuer ruleEnqueuer = A.Fake<IRuleEnqueuer>();
    private readonly DomainId ruleId = DomainId.NewGuid();
    private readonly RuleDomainObject sut;

    protected override DomainId Id
    {
        get => DomainId.Combine(AppId, ruleId);
    }

    [RuleAction]
    public sealed record TestAction : RuleAction
    {
        public int Value { get; set; }
    }

    public RuleDomainObjectTests()
    {
        var log = A.Fake<ILogger<RuleDomainObject>>();

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(AppProvider)
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

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Update_should_create_events_and_update_trigger_and_action()
    {
        var command = MakeUpdateCommand();

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Enable_should_create_events_and_update_enabled_flag()
    {
        var command = new EnableRule();

        await ExecuteCreateAsync();
        await ExecuteDisableAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
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

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Disable_should_create_events_and_update_enabled_flag()
    {
        var command = new DisableRule();

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Disable_via_update_should_create_events_and_update_enabled_flag()
    {
        var command = new UpdateRule
        {
            IsEnabled = false
        };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Delete_should_create_events_and_update_deleted_flag()
    {
        var command = new DeleteRule();

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual, None.Value);
    }

    [Fact]
    public async Task Trigger_should_invoke_rule_enqueue_but_not_change_snapshot()
    {
        var command = new TriggerRule();

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual, None.Value);

        A.CallTo(() => ruleEnqueuer.EnqueueAsync(sut.Snapshot.Id, sut.Snapshot,
                A<Envelope<IEvent>>.That.Matches(x => x.Payload is RuleManuallyTriggered)))
            .MustHaveHappened();
    }

    private Task ExecuteCreateAsync()
    {
        return PublishAsync(sut, MakeCreateCommand());
    }

    private Task ExecuteDisableAsync()
    {
        return PublishAsync(sut, new DisableRule());
    }

    private Task ExecuteDeleteAsync()
    {
        return PublishAsync(sut, new DeleteRule());
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

    protected override IAggregateCommand CreateCommand(IAggregateCommand command)
    {
        ((RuleCommand)command).RuleId = ruleId;

        return base.CreateCommand(command);
    }

    private async Task VerifySutAsync(object? actual, object? expected = null)
    {
        if (expected == null)
        {
            actual.Should().BeEquivalentTo(sut.Snapshot, o => o.IncludingProperties());
        }
        else
        {
            actual.Should().BeEquivalentTo(expected);
        }

        Assert.Equal(AppId, sut.Snapshot.AppId);

        await Verify(new { sut, events = LastEvents });
    }
}
