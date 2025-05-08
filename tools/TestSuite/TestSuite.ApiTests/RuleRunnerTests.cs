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

public class RuleRunnerTests(ClientFixture fixture, WebhookCatcherFixture webhookCatcher) : IClassFixture<ClientFixture>, IClassFixture<WebhookCatcherFixture>
{
    private readonly string secret = Guid.NewGuid().ToString();
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";
    private readonly string schemaNameRef = $"schema-{Guid.NewGuid()}-ref";
    private readonly string contentString = Guid.NewGuid().ToString();
    private readonly Guid stepId1 = Guid.NewGuid();
    private readonly Guid stepId2 = Guid.NewGuid();
    private readonly Guid stepId3 = Guid.NewGuid();
    private readonly WebhookCatcherClient webhookCatcher = webhookCatcher.Client;

    public ClientFixture _ { get; } = fixture;

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
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new WebhookFlowStepDto
                        {
                            Url = new Uri(url),
                            // Also test the secret in this case.
                            SharedSecret = secret
                        }
                    }
                }
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
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new WebhookFlowStepDto
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
                        }
                    }
                }
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
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new ScriptFlowStepDto
                        {
                            Script = $@"
                                postJSON('{url}', {{ schemaName: event.schemaId.Name }}, function () {{}})
                            "
                        }
                    }
                }
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
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new WebhookFlowStepDto
                        {
                            Url = new Uri(url),
                            // Also test the secret in this case.
                            SharedSecret = secret
                        }
                    }
                }
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
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new ScriptFlowStepDto
                        {
                            Script = @"
                                getAssetBlurHash(event, function (blurHash) {
                                    var metadata = { ...event.metadata, blurHash };

                                    updateAsset(event, metadata);
                                });
                                "
                        }
                    }
                }
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
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new WebhookFlowStepDto
                        {
                            Url = new Uri(url),
                            // Also test the secret in this case.
                            SharedSecret = secret
                        }
                    }
                }
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
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new WebhookFlowStepDto
                        {
                            Url = new Uri(url),
                            // Also test the secret in this case.
                            SharedSecret = secret
                        }
                    }
                }
            },
            Trigger = new ManualRuleTriggerDto()
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 3: Trigger rule.
        await app.Rules.TriggerRuleAsync(rule.Id, new TriggerRuleDto());

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
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new WebhookFlowStepDto
                        {
                            Url = new Uri(url),
                            // Also test the secret in this case.
                            SharedSecret = secret
                        }
                    }
                }
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

    [Fact]
    public async Task Should_log_to_console_in_rules()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Create rule.
        var createRule = new CreateRuleDto
        {
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new ScriptFlowStepDto
                        {
                            Script = @"
                                console.debug('Hello debug');
                                console.error('Hello error');
                                console.info('Hello info');                                
                                console.log('Hello Log');
                                console.warn('Hello warn');

                            "
                        }
                    }
                }
            },
            Trigger = new ManualRuleTriggerDto()
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 2: Create test content.
        await app.Rules.TriggerRuleAsync(rule.Id, new TriggerRuleDto());


        // STEP 3: Get events.
        var @event = await app.Rules.PollEventAsync(rule.Id, x => x.FlowState.Status == FlowExecutionStatus.Completed);

        await Verify(@event);
    }

    [Fact]
    public async Task Should_run_rule_with_condition()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Create rule.
        var createRule = new CreateRuleDto
        {
            Flow = new FlowDefinitionDto
            {
                InitialStepId = stepId1,
                Steps = new Dictionary<string, FlowStepDefinitionDto>
                {
                    [stepId1.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new IfFlowStepDto
                        {
                            Branches =
                            [
                                new IfFlowBranch
                                {
                                    Condition = "event.value.testValue == 1",
                                    NextStepId = stepId2,
                                },
                                new IfFlowBranch
                                {
                                    Condition = "event.value.testValue == 2",
                                    NextStepId = stepId3,
                                },
                            ]
                        }
                    },
                    [stepId2.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new ScriptFlowStepDto
                        {
                            Script = @"
                                console.info('Hello from Branch1');

                            "
                        }
                    },
                    [stepId3.ToString()] = new FlowStepDefinitionDto
                    {
                        Step = new ScriptFlowStepDto
                        {
                            Script = @"
                                console.info('Hello from Branch2');

                            "
                        }
                    }
                }
            },
            Trigger = new ManualRuleTriggerDto()
        };

        var rule = await app.Rules.PostRuleAsync(createRule);


        // STEP 2: Trigger rules 1.
        await app.Rules.TriggerRuleAsync(rule.Id, new TriggerRuleDto
        {
            Value = new Dictionary<string, int>
            {
                ["testValue"] = 1
            }
        });

        var @event1 = await app.Rules.PollEventAsync(rule.Id, x => x.FlowState.Status == FlowExecutionStatus.Completed);

        var allLogs1 =
            @event1?.FlowState.Steps.Values
                .SelectMany(x => x.Attempts)
                .SelectMany(x => x.Log) ?? [];

        Assert.Contains(allLogs1, x => x.Message.Contains("Hello from Branch1", StringComparison.Ordinal));


        // STEP 3: Trigger rules 2.
        await app.Rules.TriggerRuleAsync(rule.Id, new TriggerRuleDto
        {
            Value = new Dictionary<string, int>
            {
                ["testValue"] = 2
            }
        });

        var @event2 = await app.Rules.PollEventAsync(rule.Id, x => x.FlowState.Status == FlowExecutionStatus.Completed && x.Id != event1!.Id);

        var allLogs2 =
            @event2?.FlowState.Steps.Values
                .SelectMany(x => x.Attempts)
                .SelectMany(x => x.Log) ?? [];

        Assert.Contains(allLogs2, x => x.Message.Contains("Hello from Branch2", StringComparison.Ordinal));
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
