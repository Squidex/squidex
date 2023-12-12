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
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards;

public class GuardRuleTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly Uri validUrl = new Uri("https://squidex.io");

    [RuleAction]
    public sealed record TestAction : RuleAction
    {
        public Uri Url { get; set; }
    }

    [Fact]
    public async Task CanCreate_should_throw_exception_if_trigger_null()
    {
        var command = CreateCommand(new CreateRule
        {
            Action = new TestAction
            {
                Url = validUrl
            },
            Trigger = null!
        });

        await ValidationAssert.ThrowsAsync(() => GuardRule.CanCreate(command, AppProvider),
            new ValidationError("Trigger is required.", "Trigger"));
    }

    [Fact]
    public async Task CanCreate_should_throw_exception_if_action_null()
    {
        var command = CreateCommand(new CreateRule
        {
            Trigger = new ContentChangedTriggerV2
            {
                Schemas = []
            },
            Action = null!
        });

        await ValidationAssert.ThrowsAsync(() => GuardRule.CanCreate(command, AppProvider),
            new ValidationError("Action is required.", "Action"));
    }

    [Fact]
    public async Task CanCreate_should_not_throw_exception_if_trigger_and_action_valid()
    {
        var command = CreateCommand(new CreateRule
        {
            Trigger = new ContentChangedTriggerV2
            {
                Schemas = []
            },
            Action = new TestAction
            {
                Url = validUrl
            }
        });

        await GuardRule.CanCreate(command, AppProvider);
    }

    [Fact]
    public async Task CanUpdate_should_not_throw_exception_if_all_properties_are_null()
    {
        var command = new UpdateRule();

        await GuardRule.CanUpdate(command, CreateRule(), AppProvider);
    }

    [Fact]
    public async Task CanUpdate_should_not_throw_exception_if_rule_has_already_this_name()
    {
        var command = new UpdateRule { Name = "MyName" };

        await GuardRule.CanUpdate(command, CreateRule(), AppProvider);
    }

    [Fact]
    public async Task CanUpdate_should_not_throw_exception_if_trigger_action_and_name_are_valid()
    {
        var command = new UpdateRule
        {
            Trigger = new ContentChangedTriggerV2
            {
                Schemas = []
            },
            Action = new TestAction
            {
                Url = validUrl
            },
            Name = "NewName"
        };

        await GuardRule.CanUpdate(command, CreateRule(), AppProvider);
    }

    private CreateRule CreateCommand(CreateRule command)
    {
        command.AppId = AppId;

        return command;
    }
}
