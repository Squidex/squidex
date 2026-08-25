// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Contents;

public class DynamicContentWorkflowTests : GivenContext
{
    private readonly DomainId simpleSchemaId = DomainId.NewGuid();
    private readonly DynamicContentWorkflows sut;

    private readonly Workflow workflow = new Workflow(
        Status.Draft,
        new Dictionary<Status, WorkflowStep>
        {
            [Status.Archived] =
                new WorkflowStep(
                    new Dictionary<Status, WorkflowTransition>
                    {
                        [Status.Draft] = WorkflowTransition.Always,
                    }.ToReadonlyDictionary(),
                    StatusColors.Archived, NoUpdate.Always, Validate: true),
            [Status.Draft] =
                new WorkflowStep(
                    new Dictionary<Status, WorkflowTransition>
                    {
                        [Status.Archived] = WorkflowTransition.Always,
                        [Status.Published] = WorkflowTransition.When("data.field.iv === 2", Role.Editor),
                    }.ToReadonlyDictionary(),
                    StatusColors.Draft),
            [Status.Published] =
                new WorkflowStep(
                    new Dictionary<Status, WorkflowTransition>
                    {
                        [Status.Archived] = WorkflowTransition.Always,
                        [Status.Draft] = WorkflowTransition.Always,
                    }.ToReadonlyDictionary(),
                    StatusColors.Published, NoUpdate.When("data.field.iv === 2", Role.Owner, Role.Editor)),
        }.ToReadonlyDictionary());

    public DynamicContentWorkflowTests()
    {
        var simpleWorkflow = new Workflow(
            Status.Draft,
            new Dictionary<Status, WorkflowStep>
            {
                [Status.Draft] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Published] = WorkflowTransition.Always,
                        }.ToReadonlyDictionary(),
                        StatusColors.Draft),
                [Status.Published] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Draft] = WorkflowTransition.Always,
                        }.ToReadonlyDictionary(),
                        StatusColors.Published),
            }.ToReadonlyDictionary(),
            ReadonlyList.Create(simpleSchemaId));

        App = App with
        {
            Workflows = Workflows.Empty.Set(workflow).Set(DomainId.NewGuid(), simpleWorkflow),
        };

        var scriptEngine = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10),
            }));

        sut = new DynamicContentWorkflows(scriptEngine, new AsyncLocalCache());
    }

    [Fact]
    public async Task Should_return_info_for_valid_status()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var info = sutWorkflow.GetInfo(Status.Draft);

        Assert.Equal(new StatusInfo(Status.Draft, StatusColors.Draft), info);
    }

    [Fact]
    public async Task Should_return_info_as_null_for_invalid_status()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var info = sutWorkflow.GetInfo(new Status("Invalid"));

        Assert.Null(info);
    }

    [Fact]
    public async Task Should_return_draft_as_initial_status()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var actual = sutWorkflow.GetInitialStatus();

        Assert.Equal(Status.Draft, actual);
    }

    [Fact]
    public async Task Should_allow_publish_on_create()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var actual = sutWorkflow.CanPublishInitial(Mocks.FrontendUser(Role.Editor));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_allow_publish_on_create_if_role_not_allowed()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var actual = sutWorkflow.CanPublishInitial(Mocks.FrontendUser(Role.Developer));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_allow_if_transition_is_valid()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Draft, 2);

        var actual = sutWorkflow.CanMoveTo(content, content.Status, Status.Published, Mocks.FrontendUser(Role.Editor));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_allow_transition_if_role_is_not_allowed()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Draft, 2);

        var actual = sutWorkflow.CanMoveTo(content, content.Status, Status.Published, Mocks.FrontendUser(Role.Developer));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_not_allow_transition_if_data_not_valid()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Draft, 4);

        var actual = sutWorkflow.CanMoveTo(content, content.Status, Status.Published, Mocks.FrontendUser(Role.Editor));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_evaluate_reused_expression_per_content()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content1 = CreateContent(Status.Draft, 2);
        var content2 = CreateContent(Status.Draft, 4);

        var user = Mocks.FrontendUser(Role.Editor);

        Assert.True(sutWorkflow.CanMoveTo(content1, content1.Status, Status.Published, user));
        Assert.False(sutWorkflow.CanMoveTo(content2, content2.Status, Status.Published, user));
        Assert.True(sutWorkflow.CanMoveTo(content1, content1.Status, Status.Published, user));
    }

    [Fact]
    public async Task Should_be_able_to_update_published()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Published, 2);

        var actual = sutWorkflow.CanUpdate(content, content.Status, Mocks.FrontendUser(Role.Developer));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_be_able_to_update_archived()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Archived, 2);

        var actual = sutWorkflow.CanUpdate(content, content.Status, Mocks.FrontendUser(Role.Developer));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_not_be_able_to_update_published_with_true_expression()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Published, 2);

        var actual = sutWorkflow.CanUpdate(content, content.Status, Mocks.FrontendUser(Role.Owner));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_be_able_to_update_published_with_false_expression()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Published, 1);

        var actual = sutWorkflow.CanUpdate(content, content.Status, Mocks.FrontendUser(Role.Owner));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_be_able_to_update_published_with_correct_roles()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Published, 2);

        var actual = sutWorkflow.CanUpdate(content, content.Status, Mocks.FrontendUser(Role.Editor));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_be_able_to_update_published_with_incorrect_roles()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Published, 1);

        var actual = sutWorkflow.CanUpdate(content, content.Status, Mocks.FrontendUser(Role.Owner));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_get_next_statuses_for_draft()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Draft, 2);

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
        };

        var actual = sutWorkflow.GetNext(content, content.Status, Mocks.FrontendUser(Role.Developer));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_limit_next_statuses_if_expression_does_not_evauate_to_true()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Draft, 4);

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
        };

        var actual = sutWorkflow.GetNext(content, content.Status, Mocks.FrontendUser(Role.Editor));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_limit_next_statuses_if_role_is_not_allowed()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Draft, 2);

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Published, StatusColors.Published),
        };

        var actual = sutWorkflow.GetNext(content, content.Status, Mocks.FrontendUser(Role.Editor));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_not_reuse_next_statuses_of_conditional_step()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var allowed = CreateContent(Status.Draft, 2);
        var denied = CreateContent(Status.Draft, 4);

        var user = Mocks.FrontendUser(Role.Editor);

        sutWorkflow.GetNext(allowed, allowed.Status, user).Should().BeEquivalentTo(new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Published, StatusColors.Published),
        });

        sutWorkflow.GetNext(denied, denied.Status, user).Should().BeEquivalentTo(new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
        });
    }

    [Fact]
    public async Task Should_get_next_statuses_for_archived()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Archived, 2);

        var expected = new[]
        {
            new StatusInfo(Status.Draft, StatusColors.Draft),
        };

        var actual = sutWorkflow.GetNext(content, content.Status, null!);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_get_next_statuses_for_published()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var content = CreateContent(Status.Published, 2);

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Draft, StatusColors.Draft),
        };

        var actual = sutWorkflow.GetNext(content, content.Status, null!);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_return_all_statuses()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Draft, StatusColors.Draft),
            new StatusInfo(Status.Published, StatusColors.Published),
        };

        var actual = sutWorkflow.GetAll();

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_return_all_statuses_for_simple_schema_workflow()
    {
        using var sutWorkflow = await GetWorkflowAsync(Schema.WithId(simpleSchemaId, "simple-schema"));

        var expected = new[]
        {
            new StatusInfo(Status.Draft, StatusColors.Draft),
            new StatusInfo(Status.Published, StatusColors.Published),
        };

        var actual = sutWorkflow.GetAll();

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_return_all_statuses_for_default_workflow_if_no_workflow_configured()
    {
        App = App with
        {
            Workflows = Workflows.Empty,
        };

        using var sutWorkflow = await GetWorkflowAsync();

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Draft, StatusColors.Draft),
            new StatusInfo(Status.Published, StatusColors.Published),
        };

        var actual = sutWorkflow.GetAll();

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_not_validate_when_not_publishing()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var actual = sutWorkflow.ShouldValidate(Status.Draft);

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_not_validate_when_publishing_but_not_enabled()
    {
        using var sutWorkflow = await GetWorkflowAsync(Schema with
        {
            Properties = new SchemaProperties { ValidateOnPublish = false },
        });

        var actual = sutWorkflow.ShouldValidate(Status.Published);

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_validate_when_publishing_and_enabled()
    {
        using var sutWorkflow = await GetWorkflowAsync(Schema with
        {
            Properties = new SchemaProperties { ValidateOnPublish = true },
        });

        var actual = sutWorkflow.ShouldValidate(Status.Published);

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_validate_when_enabled_in_step()
    {
        using var sutWorkflow = await GetWorkflowAsync();

        var actual = sutWorkflow.ShouldValidate(Status.Archived);

        Assert.True(actual);
    }

    private ValueTask<IContentWorkflow> GetWorkflowAsync(Schema? schema = null)
    {
        return sut.GetWorkflowAsync(App, schema ?? Schema, CancellationToken);
    }

    private EnrichedContent CreateContent(Status status, int value)
    {
        return CreateContent() with
        {
            Status = status,
            Data =
                new ContentData()
                    .AddField("field",
                        new ContentFieldData()
                            .AddInvariant(value)),
        };
    }
}
