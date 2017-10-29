// ==========================================================================
//  RuleTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Rules
{
    public class RuleTests
    {
        private readonly JsonSerializer serializer = TestData.DefaultSerializer();
        private readonly Rule sut = new Rule(new ContentChangedTrigger(), new WebhookAction());

        public sealed class OtherTrigger : RuleTrigger
        {
            public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
            {
                throw new NotSupportedException();
            }
        }

        public sealed class OtherAction : RuleAction
        {
            public override T Accept<T>(IRuleActionVisitor<T> visitor)
            {
                throw new NotSupportedException();
            }
        }

        [Fact]
        public void Should_create_with_trigger_and_action()
        {
            var ruleTrigger = new ContentChangedTrigger();
            var ruleAction = new WebhookAction();

            var newRule = new Rule(ruleTrigger, ruleAction);

            Assert.Equal(ruleTrigger, newRule.Trigger);
            Assert.Equal(ruleAction, newRule.Action);
            Assert.True(newRule.IsEnabled);
        }

        [Fact]
        public void Should_set_enabled_to_true_when_enabling()
        {
            sut.Enable();

            Assert.True(sut.IsEnabled);
        }

        [Fact]
        public void Should_set_enabled_to_false_when_disabling()
        {
            sut.Enable();
            sut.Disable();

            Assert.False(sut.IsEnabled);
        }

        [Fact]
        public void Should_replace_trigger_when_updating()
        {
            var newTrigger = new ContentChangedTrigger();

            sut.Update(newTrigger);

            Assert.Same(newTrigger, sut.Trigger);
        }

        [Fact]
        public void Should_throw_exception_when_new_trigger_has_other_type()
        {
            Assert.Throws<ArgumentException>(() => sut.Update(new OtherTrigger()));
        }

        [Fact]
        public void Should_replace_action_when_updating()
        {
            var newAction = new WebhookAction();

            sut.Update(newAction);

            Assert.Same(newAction, sut.Action);
        }

        [Fact]
        public void Should_throw_exception_when_new_action_has_other_type()
        {
            Assert.Throws<ArgumentException>(() => sut.Update(new OtherAction()));
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            sut.Disable();

            var appClients = JToken.FromObject(sut, serializer).ToObject<Rule>(serializer);

            appClients.ShouldBeEquivalentTo(sut);
        }
    }
}
