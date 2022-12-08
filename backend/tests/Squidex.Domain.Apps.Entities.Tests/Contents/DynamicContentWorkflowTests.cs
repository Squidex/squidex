// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Contents;

public class DynamicContentWorkflowTests
{
    private readonly IAppEntity app;
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly NamedId<DomainId> simpleSchemaId = NamedId.Of(DomainId.NewGuid(), "my-simple-schema");
    private readonly DynamicContentWorkflow sut;

    private readonly Workflow workflow = new Workflow(
        Status.Draft,
        new Dictionary<Status, WorkflowStep>
        {
            [Status.Archived] =
                new WorkflowStep(
                    new Dictionary<Status, WorkflowTransition>
                    {
                        [Status.Draft] = WorkflowTransition.Always
                    }.ToReadonlyDictionary(),
                    StatusColors.Archived, NoUpdate.Always, Validate: true),
            [Status.Draft] =
                new WorkflowStep(
                    new Dictionary<Status, WorkflowTransition>
                    {
                        [Status.Archived] = WorkflowTransition.Always,
                        [Status.Published] = WorkflowTransition.When("data.field.iv === 2", "Editor")
                    }.ToReadonlyDictionary(),
                    StatusColors.Draft),
            [Status.Published] =
                new WorkflowStep(
                    new Dictionary<Status, WorkflowTransition>
                    {
                        [Status.Archived] = WorkflowTransition.Always,
                        [Status.Draft] = WorkflowTransition.Always
                    }.ToReadonlyDictionary(),
                    StatusColors.Published, NoUpdate.When("data.field.iv === 2", "Owner", "Editor"))
        }.ToReadonlyDictionary());

    public DynamicContentWorkflowTests()
    {
        app = Mocks.App(appId);

        var simpleWorkflow = new Workflow(
            Status.Draft,
            new Dictionary<Status, WorkflowStep>
            {
                [Status.Draft] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Published] = WorkflowTransition.Always
                        }.ToReadonlyDictionary(),
                        StatusColors.Draft),
                [Status.Published] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Draft] = WorkflowTransition.Always
                        }.ToReadonlyDictionary(),
                        StatusColors.Published)
            }.ToReadonlyDictionary(),
            ReadonlyList.Create(simpleSchemaId.Id));

        var workflows = Workflows.Empty.Set(workflow).Set(DomainId.NewGuid(), simpleWorkflow);

        A.CallTo(() => appProvider.GetAppAsync(appId.Id, false, default))
            .Returns(app);

        A.CallTo(() => app.Workflows)
            .Returns(workflows);

        var scriptEngine = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            }));

        sut = new DynamicContentWorkflow(scriptEngine, appProvider);
    }

    [Fact]
    public async Task Should_return_info_for_valid_status()
    {
        var content = CreateContent(Status.Draft, 2);

        var info = await sut.GetInfoAsync(content, Status.Draft);

        Assert.Equal(new StatusInfo(Status.Draft, StatusColors.Draft), info);
    }

    [Fact]
    public async Task Should_return_info_as_null_for_invalid_status()
    {
        var content = CreateContent(Status.Draft, 2);

        var info = await sut.GetInfoAsync(content, new Status("Invalid"));

        Assert.Null(info);
    }

    [Fact]
    public async Task Should_return_draft_as_initial_status()
    {
        var actual = await sut.GetInitialStatusAsync(Mocks.Schema(appId, schemaId));

        Assert.Equal(Status.Draft, actual);
    }

    [Fact]
    public async Task Should_allow_publish_on_create()
    {
        var actual = await sut.CanPublishInitialAsync(Mocks.Schema(appId, schemaId), Mocks.FrontendUser("Editor"));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_allow_publish_on_create_if_role_not_allowed()
    {
        var actual = await sut.CanPublishInitialAsync(Mocks.Schema(appId, schemaId), Mocks.FrontendUser("Developer"));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_allow_if_transition_is_valid()
    {
        var content = CreateContent(Status.Draft, 2);

        var actual = await sut.CanMoveToAsync(Mocks.Schema(appId, schemaId), content.Status, Status.Published, content.Data, Mocks.FrontendUser("Editor"));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_allow_if_transition_is_valid_for_content()
    {
        var content = CreateContent(Status.Draft, 2);

        var actual = await sut.CanMoveToAsync(content, content.Status, Status.Published, Mocks.FrontendUser("Editor"));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_allow_transition_if_role_is_not_allowed()
    {
        var content = CreateContent(Status.Draft, 2);

        var actual = await sut.CanMoveToAsync(content, content.Status, Status.Published, Mocks.FrontendUser("Developer"));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_allow_transition_if_role_is_allowed()
    {
        var content = CreateContent(Status.Draft, 2);

        var actual = await sut.CanMoveToAsync(content, content.Status, Status.Published, Mocks.FrontendUser("Editor"));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_allow_transition_if_data_not_valid()
    {
        var content = CreateContent(Status.Draft, 4);

        var actual = await sut.CanMoveToAsync(content, content.Status, Status.Published, Mocks.FrontendUser("Editor"));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_be_able_to_update_published()
    {
        var content = CreateContent(Status.Published, 2);

        var actual = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Developer"));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_be_able_to_update_draft()
    {
        var content = CreateContent(Status.Published, 2);

        var actual = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Developer"));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_be_able_to_update_archived()
    {
        var content = CreateContent(Status.Archived, 2);

        var actual = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Developer"));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_not_be_able_to_update_published_with_true_expression()
    {
        var content = CreateContent(Status.Published, 2);

        var actual = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Owner"));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_be_able_to_update_published_with_false_expression()
    {
        var content = CreateContent(Status.Published, 1);

        var actual = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Owner"));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_be_able_to_update_published_with_correct_roles()
    {
        var content = CreateContent(Status.Published, 2);

        var actual = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Editor"));

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_be_able_to_update_published_with_incorrect_roles()
    {
        var content = CreateContent(Status.Published, 1);

        var actual = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Owner"));

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_get_next_statuses_for_draft()
    {
        var content = CreateContent(Status.Draft, 2);

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived)
        };

        var actual = await sut.GetNextAsync(content, content.Status, Mocks.FrontendUser("Developer"));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_limit_next_statuses_if_expression_does_not_evauate_to_true()
    {
        var content = CreateContent(Status.Draft, 4);

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived)
        };

        var actual = await sut.GetNextAsync(content, content.Status, Mocks.FrontendUser("Editor"));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_limit_next_statuses_if_role_is_not_allowed()
    {
        var content = CreateContent(Status.Draft, 2);

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Published, StatusColors.Published)
        };

        var actual = await sut.GetNextAsync(content, content.Status, Mocks.FrontendUser("Editor"));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_get_next_statuses_for_archived()
    {
        var content = CreateContent(Status.Archived, 2);

        var expected = new[]
        {
            new StatusInfo(Status.Draft, StatusColors.Draft)
        };

        var actual = await sut.GetNextAsync(content, content.Status, null!);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_get_next_statuses_for_published()
    {
        var content = CreateContent(Status.Published, 2);

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Draft, StatusColors.Draft)
        };

        var actual = await sut.GetNextAsync(content, content.Status, null!);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_return_all_statuses()
    {
        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Draft, StatusColors.Draft),
            new StatusInfo(Status.Published, StatusColors.Published)
        };

        var actual = await sut.GetAllAsync(Mocks.Schema(appId, schemaId));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_return_all_statuses_for_simple_schema_workflow()
    {
        var expected = new[]
        {
            new StatusInfo(Status.Draft, StatusColors.Draft),
            new StatusInfo(Status.Published, StatusColors.Published)
        };

        var actual = await sut.GetAllAsync(Mocks.Schema(appId, simpleSchemaId));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_return_all_statuses_for_default_workflow_if_no_workflow_configured()
    {
        A.CallTo(() => app.Workflows).Returns(Workflows.Empty);

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Draft, StatusColors.Draft),
            new StatusInfo(Status.Published, StatusColors.Published)
        };

        var actual = await sut.GetAllAsync(Mocks.Schema(appId, simpleSchemaId));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_not_validate_when_not_publishing()
    {
        var actual = await sut.ShouldValidateAsync(Mocks.Schema(appId, schemaId), Status.Draft);

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_not_validate_when_publishing_but_not_enabled()
    {
        var actual = await sut.ShouldValidateAsync(CreateSchema(false), Status.Published);

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_validate_when_publishing_and_enabled()
    {
        var actual = await sut.ShouldValidateAsync(CreateSchema(true), Status.Published);

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_validate_when_enabled_in_step()
    {
        var actual = await sut.ShouldValidateAsync(Mocks.Schema(appId, schemaId), Status.Archived);

        Assert.True(actual);
    }

    private ISchemaEntity CreateSchema(bool validateOnPublish)
    {
        var schema = new Schema("my-schema", new SchemaProperties
        {
            ValidateOnPublish = validateOnPublish
        });

        return Mocks.Schema(appId, simpleSchemaId, schema);
    }

    private ContentEntity CreateContent(Status status, int value, bool simple = false)
    {
        var content = new ContentEntity { AppId = appId, Status = status };

        if (simple)
        {
            content.SchemaId = simpleSchemaId;
        }
        else
        {
            content.SchemaId = schemaId;
        }

        content.Data =
            new ContentData()
                .AddField("field",
                    new ContentFieldData()
                        .AddInvariant(value));

        return content;
    }
}
