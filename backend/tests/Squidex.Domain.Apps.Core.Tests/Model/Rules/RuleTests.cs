// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Rules
{
    public class RuleTests
    {
        public static readonly List<object[]> Triggers =
            typeof(Rule).Assembly.GetTypes()
                .Where(x => x.BaseType == typeof(RuleTrigger))
                .Select(Activator.CreateInstance)
                .Select(x => new[] { x })
                .ToList()!;

        private readonly Rule rule_0 = new Rule(new ContentChangedTriggerV2(), new TestAction1());

        public sealed record OtherTrigger : RuleTrigger
        {
            public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
            {
                throw new NotSupportedException();
            }
        }

        public sealed record MigratedTrigger : RuleTrigger, IMigrated<RuleTrigger>
        {
            public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
            {
                throw new NotSupportedException();
            }

            public RuleTrigger Migrate()
            {
                return new OtherTrigger();
            }
        }

        [TypeName(nameof(TestAction1))]
        public sealed record TestAction1 : RuleAction
        {
            public string Property { get; set; }
        }

        [TypeName(nameof(TestAction2))]
        public sealed record TestAction2 : RuleAction
        {
            public string Property { get; set; }
        }

        [Fact]
        public void Should_create_with_trigger_and_action()
        {
            var ruleTrigger = new ContentChangedTriggerV2();
            var ruleAction = new TestAction1();

            var newRule = new Rule(ruleTrigger, ruleAction);

            Assert.Equal(ruleTrigger, newRule.Trigger);
            Assert.Equal(ruleAction, newRule.Action);
            Assert.True(newRule.IsEnabled);
        }

        [Fact]
        public void Should_set_enabled_to_true_if_enabling()
        {
            var rule_1 = rule_0.Disable();
            var rule_2 = rule_1.Enable();
            var rule_3 = rule_2.Enable();

            Assert.NotSame(rule_1, rule_2);

            Assert.False(rule_1.IsEnabled);

            Assert.True(rule_2.IsEnabled);
            Assert.True(rule_3.IsEnabled);

            Assert.Same(rule_2, rule_3);
        }

        [Fact]
        public void Should_set_enabled_to_false_if_disabling()
        {
            var rule_1 = rule_0.Disable();
            var rule_2 = rule_1.Disable();

            Assert.NotSame(rule_0, rule_1);

            Assert.False(rule_1.IsEnabled);
            Assert.False(rule_2.IsEnabled);

            Assert.Same(rule_1, rule_2);
        }

        [Fact]
        public void Should_replace_name_if_renaming()
        {
            var rule_1 = rule_0.Rename("MyName");
            var rule_2 = rule_1.Rename("MyName");

            Assert.NotSame(rule_0, rule_1);

            Assert.Equal("MyName", rule_1.Name);
            Assert.Equal("MyName", rule_2.Name);
            Assert.Same(rule_1, rule_2);
        }

        [Fact]
        public void Should_replace_trigger_if_updating()
        {
            var newTrigger1 = new ContentChangedTriggerV2 { HandleAll = true };
            var newTrigger2 = new ContentChangedTriggerV2 { HandleAll = true };

            var rule_1 = rule_0.Update(newTrigger1);
            var rule_2 = rule_1.Update(newTrigger2);

            Assert.NotSame(rule_0.Action, newTrigger1);
            Assert.NotSame(rule_0, rule_1);
            Assert.Same(newTrigger1, rule_1.Trigger);
            Assert.Same(newTrigger1, rule_2.Trigger);
            Assert.Same(rule_1, rule_2);
        }

        [Fact]
        public void Should_throw_exception_if_new_trigger_has_other_type()
        {
            Assert.Throws<ArgumentException>(() => rule_0.Update(new OtherTrigger()));
        }

        [Fact]
        public void Should_replace_action_if_updating()
        {
            var newAction1 = new TestAction1 { Property = "NewValue" };
            var newAction2 = new TestAction1 { Property = "NewValue" };

            var rule_1 = rule_0.Update(newAction1);
            var rule_2 = rule_1.Update(newAction2);

            Assert.NotSame(rule_0.Action, newAction1);
            Assert.NotSame(rule_0, rule_1);
            Assert.Same(newAction1, rule_1.Action);
            Assert.Same(newAction1, rule_2.Action);
            Assert.Same(rule_1, rule_2);
        }

        [Fact]
        public void Should_throw_exception_if_new_action_has_other_type()
        {
            Assert.Throws<ArgumentException>(() => rule_0.Update(new TestAction2()));
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var rule_1 = rule_0.Disable().Rename("MyName");

            var serialized = rule_1.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(rule_1);
        }

        [Fact]
        public void Should_serialize_and_deserialize_and_migrate_trigger()
        {
            var rule_X = new Rule(new MigratedTrigger(), new TestAction1());

            var serialized = rule_X.SerializeAndDeserialize();

            Assert.IsType<OtherTrigger>(serialized.Trigger);
        }
    }
}
