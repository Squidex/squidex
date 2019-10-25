﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleGrainTests : HandlerTestBase<RuleState>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IRuleEnqueuer ruleEnqueuer = A.Fake<IRuleEnqueuer>();
        private readonly Guid ruleId = Guid.NewGuid();
        private readonly RuleGrain sut;

        protected override Guid Id
        {
            get { return ruleId; }
        }

        public sealed class TestAction : RuleAction
        {
            public Uri Url { get; set; }
        }

        public RuleGrainTests()
        {
            sut = new RuleGrain(Store, A.Dummy<ISemanticLog>(), appProvider, ruleEnqueuer);
            sut.ActivateAsync(Id).Wait();
        }

        [Fact]
        public async Task Command_should_throw_exception_if_rule_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainException>(ExecuteDisableAsync);
        }

        [Fact]
        public async Task Create_should_create_events_and_update_state()
        {
            var command = MakeCreateCommand();

            var result = await sut.ExecuteAsync(CreateRuleCommand(command));

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
        public async Task Update_should_create_events_and_update_state()
        {
            var command = MakeUpdateCommand();

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateRuleCommand(command));

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
        public async Task Enable_should_handle_command()
        {
            await sut.ExecuteAsync(CreateRuleCommand(MakeCreateCommand()));
            await sut.ExecuteAsync(CreateRuleCommand(new DisableRule()));
        }

        [Fact]
        public async Task Enable_should_create_events_and_update_state()
        {
            var command = new EnableRule();

            await ExecuteCreateAsync();
            await ExecuteDisableAsync();

            var result = await sut.ExecuteAsync(CreateRuleCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(sut.Snapshot.RuleDef.IsEnabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleEnabled())
                );
        }

        [Fact]
        public async Task Disable_should_create_events_and_update_state()
        {
            var command = new DisableRule();

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateRuleCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(sut.Snapshot.RuleDef.IsEnabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleDisabled())
                );
        }

        [Fact]
        public async Task Delete_should_update_create_events()
        {
            var command = new DeleteRule();

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateRuleCommand(command));

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

            var result = await sut.ExecuteAsync(CreateRuleCommand(command));

            Assert.Null(result.Value);

            A.CallTo(() => ruleEnqueuer.Enqueue(sut.Snapshot.RuleDef, sut.Id,
                A<Envelope<IEvent>>.That.Matches(x => x.Payload is RuleManuallyTriggered)))
                .MustHaveHappened();
        }

        private Task ExecuteCreateAsync()
        {
            return sut.ExecuteAsync(CreateRuleCommand(MakeCreateCommand()));
        }

        private Task ExecuteDisableAsync()
        {
            return sut.ExecuteAsync(CreateRuleCommand(new DisableRule()));
        }

        private Task ExecuteDeleteAsync()
        {
            return sut.ExecuteAsync(CreateRuleCommand(new DeleteRule()));
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
            var newTrigger = new ContentChangedTriggerV2
            {
                Schemas = ReadOnlyCollection.Empty<ContentChangedTriggerSchemaV2>()
            };

            var newAction = new TestAction
            {
                Url = new Uri("https://squidex.io/v2")
            };

            return new CreateRule { Trigger = newTrigger, Action = newAction };
        }

        private static UpdateRule MakeUpdateCommand()
        {
            var newTrigger = new ContentChangedTriggerV2
            {
                Schemas = ReadOnlyCollection.Empty<ContentChangedTriggerSchemaV2>()
            };

            var newAction = new TestAction
            {
                Url = new Uri("https://squidex.io/v2")
            };

            return new UpdateRule { Trigger = newTrigger, Action = newAction, Name = "NewName" };
        }
    }
}