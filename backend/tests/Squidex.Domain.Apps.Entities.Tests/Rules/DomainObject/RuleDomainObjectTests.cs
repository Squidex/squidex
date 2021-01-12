// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject
{
    public class RuleDomainObjectTests : HandlerTestBase<RuleDomainObject.State>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IRuleEnqueuer ruleEnqueuer = A.Fake<IRuleEnqueuer>();
        private readonly DomainId ruleId = DomainId.NewGuid();
        private readonly RuleDomainObject sut;

        protected override DomainId Id
        {
            get { return DomainId.Combine(AppId, ruleId); }
        }

        public sealed class TestAction : RuleAction
        {
            public int Value { get; set; }
        }

        public RuleDomainObjectTests()
        {
            sut = new RuleDomainObject(Store, A.Dummy<ISemanticLog>(), appProvider, ruleEnqueuer);
            sut.Setup(Id);
        }

        [Fact]
        public async Task Command_should_throw_exception_if_rule_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainException>(ExecuteDisableAsync);
        }

        [Fact]
        public async Task Create_should_create_events_and_set_intitial_state()
        {
            var command = MakeCreateCommand();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(AppId, sut.Snapshot.AppId.Id);

            Assert.Same(command.Trigger, sut.Snapshot.RuleDef.Trigger);
            Assert.Same(command.Action, sut.Snapshot.RuleDef.Action);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleCreated { Trigger = command.Trigger, Action = command.Action })
                );
        }

        [Fact]
        public async Task Update_should_create_events_and_update_trigger_and_action()
        {
            var command = MakeUpdateCommand();

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.Same(command.Trigger, sut.Snapshot.RuleDef.Trigger);
            Assert.Same(command.Action, sut.Snapshot.RuleDef.Action);

            Assert.Equal(command.Name, sut.Snapshot.RuleDef.Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleUpdated { Trigger = command.Trigger, Action = command.Action, Name = "NewName" })
                );
        }

        [Fact]
        public async Task Enable_should_create_events_and_update_enabled_flag()
        {
            var command = new EnableRule();

            await ExecuteCreateAsync();
            await ExecuteDisableAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(sut.Snapshot.RuleDef.IsEnabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleEnabled())
                );
        }

        [Fact]
        public async Task Disable_should_create_events_and_update_enabled_flag()
        {
            var command = new DisableRule();

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(sut.Snapshot.RuleDef.IsEnabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleDisabled())
                );
        }

        [Fact]
        public async Task Delete_should_create_events_and_update_deleted_flag()
        {
            var command = new DeleteRule();

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(new EntitySavedResult(1));

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

        protected T CreateRuleEvent<T>(T @event) where T : RuleEvent
        {
            @event.RuleId = ruleId;

            return CreateEvent(@event);
        }

        protected T CreateRuleCommand<T>(T command) where T : RuleCommand
        {
            command.RuleId = ruleId;

            return CreateCommand(command);
        }

        private static CreateRule MakeCreateCommand()
        {
            var newTrigger = new ContentChangedTriggerV2();
            var newAction = new TestAction { Value = 123 };

            return new CreateRule { Trigger = newTrigger, Action = newAction };
        }

        private static UpdateRule MakeUpdateCommand()
        {
            var newTrigger = new ContentChangedTriggerV2 { HandleAll = true };
            var newAction = new TestAction { Value = 456 };

            return new UpdateRule { Trigger = newTrigger, Action = newAction, Name = "NewName" };
        }

        private async Task<object?> PublishIdempotentAsync(RuleCommand command)
        {
            var result = await PublishAsync(command);

            var previousSnapshot = sut.Snapshot;
            var previousVersion = sut.Snapshot.Version;

            await PublishAsync(command);

            Assert.Same(previousSnapshot, sut.Snapshot);
            Assert.Equal(previousVersion, sut.Snapshot.Version);

            return result;
        }

        private async Task<object?> PublishAsync(RuleCommand command)
        {
            var result = await sut.ExecuteAsync(CreateRuleCommand(command));

            return result;
        }
    }
}