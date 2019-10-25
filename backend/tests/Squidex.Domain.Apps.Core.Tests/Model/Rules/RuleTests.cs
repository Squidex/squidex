﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
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

        public sealed class OtherTrigger : RuleTrigger
        {
            public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
            {
                throw new NotSupportedException();
            }
        }

        public sealed class MigratedTrigger : RuleTrigger, IMigrated<RuleTrigger>
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
        public sealed class TestAction1 : RuleAction
        {
            public string Property { get; set; }
        }

        [TypeName(nameof(TestAction2))]
        public sealed class TestAction2 : RuleAction
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
        public void Should_set_enabled_to_true_when_enabling()
        {
            var rule_1 = rule_0.Disable();
            var rule_2 = rule_1.Enable();
            var rule_3 = rule_2.Enable();

            Assert.False(rule_1.IsEnabled);
            Assert.True(rule_3.IsEnabled);
        }

        [Fact]
        public void Should_set_enabled_to_false_when_disabling()
        {
            var rule_1 = rule_0.Disable();
            var rule_2 = rule_1.Disable();

            Assert.True(rule_0.IsEnabled);
            Assert.False(rule_2.IsEnabled);
        }

        [Fact]
        public void Should_replace_name_when_renaming()
        {
            var rule_1 = rule_0.Rename("MyName");

            Assert.Equal("MyName", rule_1.Name);
        }

        [Fact]
        public void Should_replace_trigger_when_updating()
        {
            var newTrigger = new ContentChangedTriggerV2();

            var rule_1 = rule_0.Update(newTrigger);

            Assert.NotSame(newTrigger, rule_0.Trigger);
            Assert.Same(newTrigger, rule_1.Trigger);
        }

        [Fact]
        public void Should_throw_exception_when_new_trigger_has_other_type()
        {
            Assert.Throws<ArgumentException>(() => rule_0.Update(new OtherTrigger()));
        }

        [Fact]
        public void Should_replace_action_when_updating()
        {
            var newAction = new TestAction1();

            var rule_1 = rule_0.Update(newAction);

            Assert.NotSame(newAction, rule_0.Action);
            Assert.Same(newAction, rule_1.Action);
        }

        [Fact]
        public void Should_throw_exception_when_new_action_has_other_type()
        {
            Assert.Throws<ArgumentException>(() => rule_0.Update(new TestAction2()));
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var rule_1 = rule_0.Disable();

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

        [Theory]
        [MemberData(nameof(Triggers))]
        public void Should_freeze_triggers(RuleTrigger trigger)
        {
            TestUtils.TestFreeze(trigger);
        }
    }
}
