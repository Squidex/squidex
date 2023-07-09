// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class RuleEventsTests : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; }

    public RuleEventsTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_cancel_event()
    {
        // STEP 0: Create app.
        var (app, rule) = await CreateAppAndRuleAsync();


        // STEP 1: Get events.
        var events_0 = await app.Rules.GetEventsAsync(rule.Id);

        Assert.Single(events_0.Items);
        Assert.Single(events_0.Items, x => x.NextAttempt != null);


        // STEP 2: Cancel event.
        await app.Rules.DeleteEventAsync(events_0.Items[0].Id);

        var events_1 = await app.Rules.GetEventsAsync(rule.Id);

        Assert.Single(events_1.Items);
        Assert.Single(events_1.Items, x => x.NextAttempt == null);
    }

    [Fact]
    public async Task Should_cancel_event_by_rule()
    {
        // STEP 0: Create app.
        var (app, rule) = await CreateAppAndRuleAsync();


        // STEP 1: Get events.
        var events_0 = await app.Rules.GetEventsAsync(rule.Id);

        Assert.Single(events_0.Items);
        Assert.Single(events_0.Items, x => x.NextAttempt != null);


        // STEP 2: Cancel event.
        await app.Rules.DeleteRuleEventsAsync(rule.Id);

        var events_1 = await app.Rules.GetEventsAsync(rule.Id);

        Assert.Single(events_1.Items);
        Assert.Single(events_1.Items, x => x.NextAttempt == null);
    }

    [Fact]
    public async Task Should_cancel_event_by_app()
    {
        // STEP 0: Create app.
        var (app, rule) = await CreateAppAndRuleAsync();


        // STEP 1: Get events.
        var events_0 = await app.Rules.GetEventsAsync(rule.Id);

        Assert.Single(events_0.Items);
        Assert.Single(events_0.Items, x => x.NextAttempt != null);


        // STEP 2: Cancel event.
        await app.Rules.DeleteEventsAsync();

        var events_1 = await app.Rules.GetEventsAsync(rule.Id);

        Assert.Single(events_1.Items);
        Assert.Single(events_1.Items, x => x.NextAttempt == null);
    }

    private async Task<(ISquidexClient App, RuleDto)> CreateAppAndRuleAsync()
    {
        var (app, _) = await _.PostAppAsync();

        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri("http://squidex.io")
            },
            Trigger = new ManualRuleTriggerDto(),
        };

        var rule = await app.Rules.PostRuleAsync(createRule);

        await app.Rules.TriggerRuleAsync(rule.Id);

        return (app, rule);
    }
}
