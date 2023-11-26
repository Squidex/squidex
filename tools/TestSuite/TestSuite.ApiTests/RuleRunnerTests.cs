// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class RuleRunnerTests : IClassFixture<ClientFixture>, IClassFixture<WebhookCatcherFixture>
{
    private readonly string secret = Guid.NewGuid().ToString();
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";
    private readonly string schemaNameRef = $"schema-{Guid.NewGuid()}-ref";
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
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Start webhook session.
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule.
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Url = new Uri(url),
                // Also test the secret in this case.
                SharedSecret = secret
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true
            }
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 3: Create test content.
        await CreateContentAsync(app);

        // Get requests.
        var request = await webhookCatcher.PollAsync(sessionId, x => x.IsPost() && x.HasContent(schemaName));

        AssertRequest(request);


        // STEP 4: Get events.
        var eventsAll = await app.Rules.GetEventsAsync(rule.Id);
        var eventsRule = await app.Rules.GetEventsAsync();

        Assert.Single(eventsAll.Items);
        Assert.Single(eventsRule.Items);
    }

    [Fact]
    public async Task Should_run_rules_on_reference_change()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Start webhook session.
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create contents.
        var referencedSchema = await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        // Create a test content.
        var referencedContents = app.Contents<TestEntity, TestEntityData>(schemaName);

        var referencedContent = await referencedContents.CreateAsync(new TestEntityData
        {
            String = contentString
        });

        var parentSchema = await TestEntityWithReferences.CreateSchemaAsync(app.Schemas, schemaNameRef);

        // Create a test content that references the other schema.
        var parentContents = app.Contents<TestEntityWithReferences, TestEntityWithReferencesData>(schemaNameRef);

        await parentContents.CreateAsync(new TestEntityWithReferencesData
        {
            References =
            [
                referencedContent.Id
            ],
        });


        // STEP 2: Create rule.
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Payload = @$"Script(
                    getReferences(event.data.{TestEntityWithReferencesData.ReferencesField}.iv, function (references) {{
                        var payload = {{
                            name: references[0].data.{TestEntityData.StringField}.iv,
                            type: event.type
                        }};
                        complete(payload);
                    }});
                )",
                Url = new Uri(url),
                // Also test the secret in this case.
                SharedSecret = secret
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                Schemas =
                [
                    new SchemaCondition
                    {
                        SchemaId = parentSchema.Id
                    },
                ],
                ReferencedSchemas =
                [
                    new SchemaCondition
                    {
                        SchemaId = referencedSchema.Id
                    },
                ]
            }
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 3: Update referenced content.
        var updatedString = Guid.NewGuid().ToString();
        var updateEvent = "ReferenceUpdated";

        await referencedContents.UpdateAsync(referencedContent.Id, new TestEntityData
        {
            String = updatedString
        });


        // Get requests.
        var request = await webhookCatcher.PollAsync(sessionId, x => x.IsPost() && x.HasContent(updatedString) && x.HasContent(updateEvent));

        AssertRequest(request);


        // STEP 4: Get events.
        var eventsAll = await app.Rules.GetEventsAsync(rule.Id);
        var eventsRule = await app.Rules.GetEventsAsync();

        Assert.NotEmpty(eventsAll.Items);
        Assert.NotEmpty(eventsRule.Items);
    }

    [Fact]
    public async Task Should_run_scripting_rule_on_content_change()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Start webhook session.
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule.
        var createRule = new CreateRuleDto
        {
            Action = new ScriptRuleActionDto
            {
                Script = $@"
                    postJSON('{url}', {{ schemaName: event.schemaId.Name }}, function () {{}})
                "
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true
            }
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 3: Create test content.
        await CreateContentAsync(app);

        // Get requests.
        var request = await webhookCatcher.PollAsync(sessionId, x => x.IsPost() && x.HasContent(schemaName));

        Assert.NotNull(request);


        // STEP 4: Get events.
        var eventsAll = await app.Rules.GetEventsAsync(rule.Id);
        var eventsRule = await app.Rules.GetEventsAsync();

        Assert.Single(eventsAll.Items);
        Assert.Single(eventsRule.Items);
    }

    [Fact]
    public async Task Should_run_rules_on_asset_change()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Start webhook session.
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule.
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Url = new Uri(url),
                // Also test the secret in this case.
                SharedSecret = secret
            },
            Trigger = new AssetChangedRuleTriggerDto()
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 3: Create test asset.
        var asset = await CreateAssetAsync(app);

        // Get requests.
        var request = await webhookCatcher.PollAsync(sessionId, x => x.IsPost() && x.HasContent(asset.FileName));

        AssertRequest(request);


        // STEP 4: Get events.
        var eventsAll = await app.Rules.GetEventsAsync(rule.Id);
        var eventsRule = await app.Rules.GetEventsAsync();

        Assert.Single(eventsAll.Items);
        Assert.Single(eventsRule.Items);
    }

    [Fact]
    public async Task Should_run_rules_on_asset_and_update_metadata()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Start webhook session.
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule.
        var createRule = new CreateRuleDto
        {
            Action = new ScriptRuleActionDto
            {
                Script = @"
                    getAssetBlurHash(event, function (blurHash) {
                        var metadata = { ...event.metadata, blurHash };

                        updateAsset(event, metadata);
                    });
                    "
            },
            Trigger = new AssetChangedRuleTriggerDto()
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 3: Create test asset.
        var asset = await CreateAssetAsync(app);

        // Get asset
        var found = await app.Assets.PollAsync(x => x.Id == asset.Id && x.Metadata.ContainsKey("blurHash"));

        Assert.NotNull(found);
    }

    [Fact]
    public async Task Should_run_rules_on_schema_change()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Start webhook session.
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule.
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Url = new Uri(url),
                // Also test the secret in this case.
                SharedSecret = secret
            },
            Trigger = new SchemaChangedRuleTriggerDto()
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 3: Create test schema.
        await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        // Get requests.
        var request = await webhookCatcher.PollAsync(sessionId, x => x.IsPost() && x.HasContent(schemaName));

        AssertRequest(request);


        // STEP 4: Get events.
        var eventsAll = await app.Rules.GetEventsAsync(rule.Id);
        var eventsRule = await app.Rules.GetEventsAsync();

        Assert.Single(eventsAll.Items);
        Assert.Single(eventsRule.Items);
    }

    [Fact]
    public async Task Should_run_rule_manually()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Start webhook session.
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create rule.
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Url = new Uri(url),
                // Also test the secret in this case.
                SharedSecret = secret
            },
            Trigger = new ManualRuleTriggerDto()
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 3: Trigger rule.
        await app.Rules.TriggerRuleAsync(rule.Id);

        // Get requests.
        var request = await webhookCatcher.PollAsync(sessionId, x => x.IsPost());

        AssertRequest(request);


        // STEP 4: Get events.
        var eventsAll = await app.Rules.GetEventsAsync(rule.Id);
        var eventsRule = await app.Rules.GetEventsAsync();

        Assert.Single(eventsAll.Items);
        Assert.Single(eventsRule.Items);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_rerun_rules(bool fromSnapshots)
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Start webhook session.
        var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


        // STEP 2: Create disabled rule.
        var createRule = new CreateRuleDto
        {
            Action = new WebhookRuleActionDto
            {
                Url = new Uri(url),
                // Also test the secret in this case.
                SharedSecret = secret
            },
            Trigger = new ContentChangedRuleTriggerDto
            {
                HandleAll = true
            }
        };

        var rule = await app.Rules.PostRuleAsync(createRule);

        // Disable rule, so that we do not create the event from the rule itself.
        await app.Rules.DisableRuleAsync(rule.Id);


        // STEP 3: Create test content before rule.
        await CreateContentAsync(app);


        // STEP 4: Run rule.
        await app.Rules.PutRuleRunAsync(rule.Id, fromSnapshots);

        // Get requests.
        var request = await webhookCatcher.PollAsync(sessionId, x => x.IsPost() && x.HasContent(schemaName));

        AssertRequest(request);
    }

    private async Task CreateContentAsync(ISquidexClient app)
    {
        await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        // Create a test content.
        var contents = app.Contents<TestEntity, TestEntityData>(schemaName);

        await contents.CreateAsync(new TestEntityData
        {
            String = contentString
        });
    }

    private static async Task<AssetDto> CreateAssetAsync(ISquidexClient app)
    {
        // Upload a test asset
        var fileInfo = new FileInfo("Assets/logo-squared.png");

        await using (var stream = fileInfo.OpenRead())
        {
            var upload = new FileParameter(stream, fileInfo.Name, "image/png");

            return await app.Assets.PostAssetAsync(file: upload);
        }
    }

    private void AssertRequest(WebhookRequest? request)
    {
        Assert.NotNull(request);
        Assert.NotNull(request?.Headers["X-Signature"]);

        Assert.Equal(request?.Headers["X-Signature"], WebhookUtils.CalculateSignature(request?.Content ?? string.Empty, secret));
    }
}
