// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Migrations;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Rules;

public class RuleTests
{
    public static readonly List<object[]> Triggers =
        typeof(Rule).Assembly.GetTypes()
            .Where(x => x.BaseType == typeof(RuleTrigger))
            .Select(Activator.CreateInstance)
            .Select(x => new[] { x })
            .ToList()!;

    private readonly Rule rule_0 = new Rule { Action = new TestAction1(), Trigger = new ContentChangedTriggerV2() };

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

    [RuleAction]
    public sealed record TestAction1 : RuleAction
    {
        public string Property { get; set; }
    }

    [RuleAction]
    public sealed record TestAction2 : RuleAction
    {
        public string Property { get; set; }
    }

    [Fact]
    public void Should_enable_rule()
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
    public void Should_disable_rule()
    {
        var rule_1 = rule_0.Disable();
        var rule_2 = rule_1.Disable();

        Assert.NotSame(rule_0, rule_1);
        Assert.False(rule_1.IsEnabled);
        Assert.False(rule_2.IsEnabled);
        Assert.Same(rule_1, rule_2);
    }

    [Fact]
    public void Should_change_category()
    {
        var newName = "MyName";

        var rule_1 = rule_0.Rename(newName);
        var rule_2 = rule_1.Rename(newName);

        Assert.NotSame(rule_0, rule_1);
        Assert.Equal(newName, rule_1.Name);
        Assert.Equal(newName, rule_2.Name);
        Assert.Same(rule_1, rule_2);
    }

    [Fact]
    public void Should_replace_trigger()
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
    public void Should_throw_exception_if_new_trigger_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => rule_0.Update((RuleTrigger)null!));
    }

    [Fact]
    public void Should_throw_exception_if_new_trigger_has_other_type()
    {
        Assert.Throws<ArgumentException>(() => rule_0.Update(new OtherTrigger()));
    }

    [Fact]
    public void Should_replace_action()
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
    public void Should_throw_exception_if_action_trigger_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => rule_0.Update((RuleAction)null!));
    }

    [Fact]
    public void Should_throw_exception_if_new_action_has_other_type()
    {
        Assert.Throws<ArgumentException>(() => rule_0.Update(new TestAction2()));
    }

    [Fact]
    public void Should_deserialize_old_state()
    {
        var original = TestUtils.DefaultSerializer.Deserialize<Rule>(File.ReadAllText("Model/Rules/Rule.json"));

        var deserialized = TestUtils.DefaultSerializer.Deserialize<Rule>(File.ReadAllText("Model/Rules/Rule_Old.json"));

        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Model/Rules/Rule.json");

        var deserialized = TestUtils.DefaultSerializer.Deserialize<Rule>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Model/Rules/Rule.json").CleanJson();

        var serialized = TestUtils.SerializeWithoutNulls(TestUtils.DefaultSerializer.Deserialize<Rule>(json));

        Assert.Equal(json, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_and_migrate_trigger()
    {
        var rule_X = new Rule { Trigger = new MigratedTrigger(), Action = new TestAction1() };

        var serialized = rule_X.SerializeAndDeserialize();

        Assert.IsType<OtherTrigger>(serialized.Trigger);
    }
}
