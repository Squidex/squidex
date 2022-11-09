// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class RuleRunnerTests : IClassFixture<ClientFixture>, IClassFixture<WebhookCatcherFixture>
{
    private readonly string secret = Guid.NewGuid().ToString();
    private readonly string appName = Guid.NewGuid().ToString();
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";
    private readonly string contentString = Guid.NewGuid().ToString();
    private readonly WebhookCatcherClient webhookCatcher;

    public ClientFixture _ { get; }

    public RuleRunnerTests(ClientFixture fixture, WebhookCatcherFixture webhookCatcher)
    {
        _ = fixture;

        this.webhookCatcher = webhookCatcher.Client;
    }

    [Fact]
    public async Task Should_run_rules_on_content_change()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Start webhook session
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                SharedSecret = secret,
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri(url)
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true
            }
        };

        var rule = await _.Rules.PostRuleAsync(appName, createRule);


        // STEP 3: Create test content
        await CreateContentAsync();

        // Get requests.
        var requests = await webhookCatcher.WaitForRequestsAsync(sessionId, TimeSpan.FromMinutes(2));
        var request = requests.FirstOrDefault(x => x.Method == "POST" && x.Content.Contains(schemaName, StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(request);
        Assert.NotNull(request.Headers["X-Signature"]);
        Assert.Equal(request.Headers["X-Signature"], WebhookUtils.CalculateSignature(request.Content, secret));


        // STEP 4: Get events
        var eventsAll = await _.Rules.GetEventsAsync(appName, rule.Id);
        var eventsRule = await _.Rules.GetEventsAsync(appName);

        Assert.Single(eventsAll.Items);
        Assert.Single(eventsRule.Items);
    }

    [Fact]
    public async Task Should_run_rules_on_asset_change()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Start webhook session
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                SharedSecret = secret,
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri(url)
            },
            Trigger = new AssetChangedRuleTriggerDto()
        };

        var rule = await _.Rules.PostRuleAsync(appName, createRule);


        // STEP 3: Create test asset
        await CreateAssetAsync();

        // Get requests.
        var requests = await webhookCatcher.WaitForRequestsAsync(sessionId, TimeSpan.FromMinutes(2));
        var request = requests.FirstOrDefault(x => x.Method == "POST" && x.Content.Contains("logo-squared", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(request);
        Assert.NotNull(request.Headers["X-Signature"]);
        Assert.Equal(request.Headers["X-Signature"], WebhookUtils.CalculateSignature(request.Content, secret));


        // STEP 4: Get events
        var eventsAll = await _.Rules.GetEventsAsync(appName, rule.Id);
        var eventsRule = await _.Rules.GetEventsAsync(appName);

        Assert.Single(eventsAll.Items);
        Assert.Single(eventsRule.Items);
    }

    [Fact]
    public async Task Should_run_rules_on_schema_change()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Start webhook session
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                SharedSecret = secret,
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri(url)
            },
            Trigger = new SchemaChangedRuleTriggerDto()
        };

        var rule = await _.Rules.PostRuleAsync(appName, createRule);


        // STEP 3: Create test schema
        await TestEntity.CreateSchemaAsync(_.Schemas, appName, schemaName);

        // Get requests.
        var requests = await webhookCatcher.WaitForRequestsAsync(sessionId, TimeSpan.FromMinutes(2));
        var request = requests.FirstOrDefault(x => x.Method == "POST" && x.Content.Contains(schemaName, StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(request);
        Assert.NotNull(request.Headers["X-Signature"]);
        Assert.Equal(request.Headers["X-Signature"], WebhookUtils.CalculateSignature(request.Content, secret));


        // STEP 4: Get events
        var eventsAll = await _.Rules.GetEventsAsync(appName, rule.Id);
        var eventsRule = await _.Rules.GetEventsAsync(appName);

        Assert.Single(eventsAll.Items);
        Assert.Single(eventsRule.Items);
    }

    [Fact]
    public async Task Should_run_rule_manually()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Start webhook session
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                SharedSecret = secret,
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri(url)
            },
            Trigger = new ManualRuleTriggerDto()
        };

        var rule = await _.Rules.PostRuleAsync(appName, createRule);


        // STEP 3: Trigger rule
        await _.Rules.TriggerRuleAsync(appName, rule.Id);

        // Get requests.
        var requests = await webhookCatcher.WaitForRequestsAsync(sessionId, TimeSpan.FromSeconds(30));
        var request = requests.FirstOrDefault(x => x.Method == "POST");

        Assert.NotNull(request);
        Assert.NotNull(request.Headers["X-Signature"]);
        Assert.Equal(request.Headers["X-Signature"], WebhookUtils.CalculateSignature(request.Content, secret));


        // STEP 4: Get events
        var eventsAll = await _.Rules.GetEventsAsync(appName, rule.Id);
        var eventsRule = await _.Rules.GetEventsAsync(appName);

        Assert.Single(eventsAll.Items);
        Assert.Single(eventsRule.Items);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_rerun_rules(bool fromSnapshots)
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Start webhook session
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create disabled rule
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Method = WebhookMethod.POST,
                Payload = null,
                PayloadType = null,
                Url = new Uri(url)
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true
            }
        };

        var rule = await _.Rules.PostRuleAsync(appName, createRule);

        // Disable rule, so that we do not create the event from the rule itself.
        await _.Rules.DisableRuleAsync(appName, rule.Id);


        // STEP 3: Create test content before rule
        await CreateContentAsync();


        // STEP 4: Run rule.
        await _.Rules.PutRuleRunAsync(appName, rule.Id, fromSnapshots);

        // Get requests.
        var requests = await webhookCatcher.WaitForRequestsAsync(sessionId, TimeSpan.FromSeconds(30));

        Assert.Contains(requests, x => x.Method == "POST" && x.Content.Contains(schemaName, StringComparison.OrdinalIgnoreCase));
    }

    private async Task CreateContentAsync()
    {
        await TestEntity.CreateSchemaAsync(_.Schemas, appName, schemaName);

        // Create a test content.
        var contents = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(appName, schemaName);

        await contents.CreateAsync(new TestEntityData
        {
            String = contentString
        });
    }

    private async Task CreateAssetAsync()
    {
        // Upload a test asset
        var fileInfo = new FileInfo("Assets/logo-squared.png");

        await using (var stream = fileInfo.OpenRead())
        {
            var upload = new FileParameter(stream, fileInfo.Name, "image/png");

            await _.Assets.PostAssetAsync(appName, file: upload);
        }
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
