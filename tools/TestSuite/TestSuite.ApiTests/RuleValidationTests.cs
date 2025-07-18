// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class RuleValidationTests(CreatedAppFixture fixture) : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_get_steps()
    {
        var steps = await _.Client.Rules.GetStepsAsync();

        Assert.NotEmpty(steps);
    }

    [Fact]
    public async Task Should_get_event_schemas()
    {
        var schema = await _.Client.Rules.GetEventSchemaAsync("EnrichedContentEvent");

        Assert.NotNull(schema);
    }

    [Fact]
    public async Task Should_get_event_types()
    {
        var eventTypes = await _.Client.Rules.GetEventTypesAsync();

        Assert.NotEmpty(eventTypes);
    }

    [Fact]
    public async Task Should_validate_valid_trigger()
    {
        var trigger = new UsageRuleTriggerDto { Limit = 500, NumDays = 5 };

        await _.Client.Rules.ValidateTriggerAsync(trigger);
    }

    [Fact]
    public async Task Should_validate_invalid_trigger()
    {
        var trigger = new UsageRuleTriggerDto { Limit = 500, NumDays = 50 };

        var ex = await Assert.ThrowsAnyAsync<SquidexException>(async () =>
        {
            await _.Client.Rules.ValidateTriggerAsync(trigger);
        });

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Should_validate_valid_step()
    {
        var trigger = new WebhookFlowStepDto { Url = "https://squidex.io" };

        await _.Client.Rules.ValidateStepAsync(trigger);
    }

    [Fact]
    public async Task Should_validate_invalid_step()
    {
        var trigger = new WebhookFlowStepDto();

        var ex = await Assert.ThrowsAnyAsync<SquidexException>(async () =>
        {
            await _.Client.Rules.ValidateStepAsync(trigger);
        });

        Assert.Equal(400, ex.StatusCode);
    }
}
