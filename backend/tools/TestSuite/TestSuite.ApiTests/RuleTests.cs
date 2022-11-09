// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
public class RuleTests : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();
    private readonly string ruleName = Guid.NewGuid().ToString();

    public ClientFixture _ { get; }

    public RuleTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_rule()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Create rule
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri("http://squidex.io")
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true
            }
        };

        var rule = await _.Rules.PostRuleAsync(appName, createRule);

        Assert.IsType<WebhookRuleActionDto>(rule.Action);

        await Verify(rule);
    }

    [Fact]
    public async Task Should_update_rule()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Create rule
        var createRequest = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri("http://squidex.io")
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true
            }
        };

        var rule_0 = await _.Rules.PostRuleAsync(appName, createRequest);


        // STEP 2: Update rule
        var updateRequest = new UpdateRuleDto
        {
            Name = ruleName
        };

        var rule_1 = await _.Rules.PutRuleAsync(appName, rule_0.Id, updateRequest);

        Assert.Equal(ruleName, rule_1.Name);

        await Verify(rule_1);
    }

    [Fact]
    public async Task Should_delete_rule()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Create rule
        var createRequest = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri("http://squidex.io")
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true
            }
        };

        var rule = await _.Rules.PostRuleAsync(appName, createRequest);


        // STEP 2: Delete rule
        await _.Rules.DeleteRuleAsync(appName, rule.Id);

        var rules = await _.Rules.GetRulesAsync(appName);

        Assert.DoesNotContain(rules.Items, x => x.Id == rule.Id);
    }

    [Fact]
    public async Task Should_get_actions()
    {
        var actions = await _.Rules.GetActionsAsync();

        Assert.NotEmpty(actions);
    }

    [Fact]
    public async Task Should_get_event_schemas()
    {
        var schema = await _.Rules.GetEventSchemaAsync("EnrichedContentEvent");

        Assert.NotNull(schema);
    }

    [Fact]
    public async Task Should_get_event_types()
    {
        var eventTypes = await _.Rules.GetEventTypesAsync();

        Assert.NotEmpty(eventTypes);
    }

    private async Task CreateAppAsync()
    {
        var createRequest = new CreateAppDto
        {
            Name = appName
        };

        await _.Apps.PostAppAsync(createRequest);
    }
}
