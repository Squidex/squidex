// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards;

public class GuardRuleTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IFlowManager<FlowEventContext> flowManager = A.Fake<IFlowManager<FlowEventContext>>();
    private readonly IRuleValidator validator;

    public GuardRuleTests()
    {
        validator = new RuleValidator(flowManager, AppProvider);
    }

    [Fact]
    public async Task CanCreate_should_throw_exception_if_trigger_is_null()
    {
        var command = CreateCommand(new CreateRule
        {
            Flow = new FlowDefinition
            {
                InitialStepId = Guid.NewGuid(),
            },
            Trigger = null!,
        });

        await ValidationAssert.ThrowsAsync(() => GuardRule.CanCreate(command, validator, CancellationToken),
            new ValidationError("Trigger is required.", "Trigger"));
    }

    [Fact]
    public async Task CanCreate_should_throw_exception_if_flow_is_null()
    {
        var command = CreateCommand(new CreateRule
        {
            Trigger = new ContentChangedTriggerV2
            {
                Schemas = [],
            },
            Flow = null!,
        });

        await ValidationAssert.ThrowsAsync(() => GuardRule.CanCreate(command, validator, CancellationToken),
            new ValidationError("Flow is required.", "Flow"));
    }

    [Fact]
    public async Task CanCreate_should_not_throw_exception_if_trigger_and_action_valid()
    {
        var command = CreateCommand(new CreateRule
        {
            Trigger = new ContentChangedTriggerV2
            {
                Schemas = [],
            },
            Flow = new FlowDefinition
            {
                InitialStepId = Guid.NewGuid(),
            },
        });

        await GuardRule.CanCreate(command, validator, CancellationToken);
    }

    [Fact]
    public async Task CanUpdate_should_not_throw_exception_if_all_properties_are_null()
    {
        var command = new UpdateRule();

        await GuardRule.CanUpdate(command, CreateRule(), validator, CancellationToken);
    }

    [Fact]
    public async Task CanUpdate_should_not_throw_exception_if_rule_has_already_this_name()
    {
        var command = new UpdateRule { Name = "MyName" };

        await GuardRule.CanUpdate(command, CreateRule(), validator, CancellationToken);
    }

    [Fact]
    public async Task CanUpdate_should_not_throw_exception_if_trigger_flow_and_name_are_valid()
    {
        var command = CreateCommand(new UpdateRule
        {
            Trigger = new ContentChangedTriggerV2
            {
                Schemas = [],
            },
            Flow = new FlowDefinition
            {
                InitialStepId = Guid.NewGuid(),
            },
            Name = "NewName",
        });

        await GuardRule.CanUpdate(command, CreateRule(), validator, CancellationToken);
    }

    private T CreateCommand<T>(T command) where T : IAppCommand
    {
        command.AppId = AppId;

        return command;
    }
}
