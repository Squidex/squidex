// ==========================================================================
//  RuleDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Immutable;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleDomainObjectTests : HandlerTestBase<RuleDomainObject>
    {
        private readonly RuleTrigger ruleTrigger = new ContentChangedTrigger();
        private readonly RuleAction ruleAction = new WebhookAction { Url = new Uri("https://squidex.io") };
        private readonly RuleDomainObject sut;

        public Guid RuleId { get; } = Guid.NewGuid();

        public RuleDomainObjectTests()
        {
            sut = new RuleDomainObject();
        }

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            sut.Create(new CreateRule { Trigger = ruleTrigger, Action = ruleAction });

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateRuleCommand(new CreateRule { Trigger = ruleTrigger, Action = ruleAction }));
            });
        }

        [Fact]
        public void Create_should_create_events()
        {
            var command = new CreateRule { Trigger = ruleTrigger, Action = ruleAction };

            sut.Create(CreateRuleCommand(command));

            Assert.Same(ruleTrigger, sut.State.RuleDef.Trigger);
            Assert.Same(ruleAction, sut.State.RuleDef.Action);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleCreated { Trigger = ruleTrigger, Action = ruleAction })
                );
        }

        [Fact]
        public void Update_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateRuleCommand(new UpdateRule { Trigger = ruleTrigger, Action = ruleAction }));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_rule_is_deleted()
        {
            CreateRule();
            DeleteRule();

            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateRuleCommand(new UpdateRule { Trigger = ruleTrigger, Action = ruleAction }));
            });
        }

        [Fact]
        public void Update_should_create_events()
        {
            var newTrigger = new ContentChangedTrigger
            {
                Schemas = ImmutableList<ContentChangedTriggerSchema>.Empty
            };

            var newAction = new WebhookAction
            {
                Url = new Uri("https://squidex.io/v2")
            };

            CreateRule();

            var command = new UpdateRule { Trigger = newTrigger, Action = newAction };

            sut.Update(CreateRuleCommand(command));

            Assert.Same(newTrigger, sut.State.RuleDef.Trigger);
            Assert.Same(newAction, sut.State.RuleDef.Action);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleUpdated { Trigger = newTrigger, Action = newAction })
                );
        }

        [Fact]
        public void Enable_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Enable(CreateRuleCommand(new EnableRule()));
            });
        }

        [Fact]
        public void Enable_should_throw_exception_if_rule_is_deleted()
        {
            CreateRule();
            DeleteRule();

            Assert.Throws<DomainException>(() =>
            {
                sut.Enable(CreateRuleCommand(new EnableRule()));
            });
        }

        [Fact]
        public void Enable_should_create_events()
        {
            CreateRule();

            var command = new EnableRule();

            sut.Enable(CreateRuleCommand(command));

            Assert.True(sut.State.RuleDef.IsEnabled);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleEnabled())
                );
        }

        [Fact]
        public void Disable_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Disable(CreateRuleCommand(new DisableRule()));
            });
        }

        [Fact]
        public void Disable_should_throw_exception_if_rule_is_deleted()
        {
            CreateRule();
            DeleteRule();

            Assert.Throws<DomainException>(() =>
            {
                sut.Disable(CreateRuleCommand(new DisableRule()));
            });
        }

        [Fact]
        public void Disable_should_create_events()
        {
            CreateRule();

            var command = new DisableRule();

            sut.Disable(CreateRuleCommand(command));

            Assert.False(sut.State.RuleDef.IsEnabled);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleDisabled())
                );
        }

        [Fact]
        public void Delete_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateRuleCommand(new DeleteRule()));
            });
        }

        [Fact]
        public void Delete_should_throw_exception_if_already_deleted()
        {
            CreateRule();
            DeleteRule();

            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateRuleCommand(new DeleteRule()));
            });
        }

        [Fact]
        public void Delete_should_update_create_events()
        {
            CreateRule();

            sut.Delete(CreateRuleCommand(new DeleteRule()));

            Assert.True(sut.State.IsDeleted);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateRuleEvent(new RuleDeleted())
                );
        }

        private void CreateRule()
        {
            sut.Create(CreateRuleCommand(new CreateRule { Trigger = ruleTrigger, Action = ruleAction }));
            sut.ClearUncommittedEvents();
        }

        private void DeleteRule()
        {
            sut.Delete(CreateRuleCommand(new DeleteRule()));
            sut.ClearUncommittedEvents();
        }

        protected T CreateRuleEvent<T>(T @event) where T : RuleEvent
        {
            @event.RuleId = RuleId;

            return CreateEvent(@event);
        }

        protected T CreateRuleCommand<T>(T command) where T : RuleAggregateCommand
        {
            command.RuleId = RuleId;

            return CreateCommand(command);
        }
    }
}