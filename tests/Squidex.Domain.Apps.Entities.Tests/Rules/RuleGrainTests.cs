// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans.Core;
using Orleans.Runtime;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleDomainObjectTests : HandlerTestBase<RuleGrain, RuleState>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly Guid ruleId = Guid.NewGuid();
        private readonly RuleGrain sut;

        public sealed class MyRuleGrain : RuleGrain
        {
            public MyRuleGrain(IStore<Guid> store, IAppProvider appProvider, IGrainIdentity identity, IGrainRuntime runtime)
                : base(store, appProvider, identity, runtime)
            {
            }
        }

        protected override Guid Id
        {
            get { return ruleId; }
        }

        public RuleDomainObjectTests()
        {
            sut = new MyRuleGrain(Store, appProvider, Identity, Runtime);
            sut.OnActivateAsync().Wait();
        }

        [Fact]
        public async Task Command_should_throw_exception_if_rule_is_deleted()
        {
            await CreateAsync();
            await DeleteAsync();

            await Assert.ThrowsAsync<DomainException>(() =>
            {
                return sut.ExecuteAsync(J(CreateRuleCommand(MakeUpdateCommand())));
            });
        }

        [Fact]
        public async Task Create_should_create_events()
        {
            var command = MakeCreateCommand();

            var result = await sut.ExecuteAsync(J(CreateRuleCommand(command)));

            Assert.True(result.Value is EntityCreatedResult<Guid>);

            Assert.Equal(AppId, sut.Snapshot.AppId.Id);

            Assert.Same(command.Trigger, sut.Snapshot.RuleDef.Trigger);
            Assert.Same(command.Action, sut.Snapshot.RuleDef.Action);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleCreated { Trigger = command.Trigger, Action = command.Action })
                );
        }

        [Fact]
        public async Task Update_should_handle_command()
        {
            await sut.ExecuteAsync(J(CreateRuleCommand(MakeCreateCommand())));

            var result = await sut.ExecuteAsync(J(CreateRuleCommand(MakeUpdateCommand())));

            Assert.True(result.Value is EntitySavedResult);
        }

        [Fact]
        public async Task Update_should_create_events()
        {
            var command = MakeUpdateCommand();

            await CreateAsync();

            var result = await sut.ExecuteAsync(J(CreateRuleCommand(command)));

            Assert.True(result.Value is EntitySavedResult);

            Assert.Same(command.Trigger, sut.Snapshot.RuleDef.Trigger);
            Assert.Same(command.Action, sut.Snapshot.RuleDef.Action);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleUpdated { Trigger = command.Trigger, Action = command.Action })
                );
        }

        [Fact]
        public async Task Enable_should_handle_command()
        {
            await sut.ExecuteAsync(J(CreateRuleCommand(MakeCreateCommand())));
            await sut.ExecuteAsync(J(CreateRuleCommand(new DisableRule())));
        }

        [Fact]
        public async Task Enable_should_create_events()
        {
            await CreateAsync();
            await sut.ExecuteAsync(J(CreateRuleCommand(new DisableRule())));

            var result = await sut.ExecuteAsync(J(CreateRuleCommand(new EnableRule())));

            Assert.True(result.Value is EntitySavedResult);

            Assert.True(sut.Snapshot.RuleDef.IsEnabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleEnabled())
                );
        }

        [Fact]
        public async Task Disable_should_create_events()
        {
            await CreateAsync();

            var result = await sut.ExecuteAsync(J(CreateRuleCommand(new DisableRule())));

            Assert.True(result.Value is EntitySavedResult);

            Assert.False(sut.Snapshot.RuleDef.IsEnabled);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleDisabled())
                );
        }

        [Fact]
        public async Task Delete_should_update_create_events()
        {
            await CreateAsync();

            var result = await sut.ExecuteAsync(J(CreateRuleCommand(new DeleteRule())));

            Assert.True(result.Value is EntitySavedResult);

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleDeleted())
                );
        }

        private Task CreateAsync()
        {
            return sut.ExecuteAsync(J(CreateRuleCommand(MakeCreateCommand())));
        }

        private Task DeleteAsync()
        {
            return sut.ExecuteAsync(J(CreateRuleCommand(new DeleteRule())));
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

        private CreateRule MakeCreateCommand()
        {
            var newTrigger = new ContentChangedTrigger
            {
                Schemas = ImmutableList<ContentChangedTriggerSchema>.Empty
            };

            var newAction = new WebhookAction
            {
                Url = new Uri("https://squidex.io/v2")
            };

            return new CreateRule { Trigger = newTrigger, Action = newAction };
        }

        private static UpdateRule MakeUpdateCommand()
        {
            var newTrigger = new ContentChangedTrigger
            {
                Schemas = ImmutableList<ContentChangedTriggerSchema>.Empty
            };

            var newAction = new WebhookAction
            {
                Url = new Uri("https://squidex.io/v2")
            };

            return new UpdateRule { Trigger = newTrigger, Action = newAction };
        }
    }
}