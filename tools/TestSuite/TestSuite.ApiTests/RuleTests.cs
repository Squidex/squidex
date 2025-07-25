﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class RuleTests(ClientFixture fixture) : IClassFixture<ClientFixture>
{
    private readonly string ruleName = Guid.NewGuid().ToString();
    private readonly Guid stepId = Guid.NewGuid();

    public ClientFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_create_rule()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Create rule.
        var createRule = new CreateRuleDto
        {
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new WebhookFlowStepDto
                        {
                            Method = WebhookMethod.POST,
                            Payload = null,
                            PayloadType = null,
                            Url = "http://squidex.io",
                        },
                    },
                },
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true,
            },
        };

        var rule = await app.Rules.PostRuleAsync(createRule);

        Assert.IsType<WebhookRuleActionDto>(rule.Action);
        await Verify(rule);
    }

    [Fact]
    public async Task Should_create_rule_with_action()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Create rule.
        var createRule = new CreateRuleDto
        {
            Flow = null!,
            Action = new WebhookRuleActionDto
            {
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri("http://squidex.io"),
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true,
            },
        };

        var rule = await app.Rules.PostRuleAsync(createRule);

        Assert.IsType<WebhookRuleActionDto>(rule.Action);
        await Verify(rule);
    }

    [Fact]
    public async Task Should_update_rule()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Create rule.
        var createRequest = new CreateRuleDto
        {
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new WebhookFlowStepDto
                        {
                            Method = WebhookMethod.POST,
                            Payload = null,
                            PayloadType = null,
                            Url = "http://squidex.io",
                        },
                    },
                },
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true,
            },
        };

        var rule_0 = await app.Rules.PostRuleAsync(createRequest);


        // STEP 2: Update rule.
        var updateRequest = new UpdateRuleDto
        {
            Name = ruleName,
        };

        var rule_1 = await app.Rules.PutRuleAsync(rule_0.Id, updateRequest);

        Assert.Equal(ruleName, rule_1.Name);
        await Verify(rule_1);
    }

    [Fact]
    public async Task Should_delete_rule()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Create rule.
        var createRequest = new CreateRuleDto
        {
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new WebhookFlowStepDto
                        {
                            Method = WebhookMethod.POST,
                            Payload = null,
                            PayloadType = null,
                            Url = "http://squidex.io",
                        },
                    },
                },
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true,
            },
        };

        var rule = await app.Rules.PostRuleAsync(createRequest);


        // STEP 2: Delete rule.
        await app.Rules.DeleteRuleAsync(rule.Id);

        var rules = await app.Rules.GetRulesAsync();

        Assert.DoesNotContain(rules.Items, x => x.Id == rule.Id);
    }
}
