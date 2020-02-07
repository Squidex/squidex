// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class DynamicContentWorkflowTests
    {
        private readonly IAppEntity app;
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly NamedId<Guid> simpleSchemaId = NamedId.Of(Guid.NewGuid(), "my-simple-schema");
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
                        },
                        StatusColors.Archived, NoUpdate.Always),
                [Status.Draft] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = WorkflowTransition.Always,
                            [Status.Published] = WorkflowTransition.When("data.field.iv === 2", "Editor")
                        },
                        StatusColors.Draft),
                [Status.Published] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = WorkflowTransition.Always,
                            [Status.Draft] = WorkflowTransition.Always
                        },
                        StatusColors.Published, NoUpdate.When("data.field.iv === 2", "Owner", "Editor"))
            });

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
                            },
                            StatusColors.Draft),
                    [Status.Published] =
                        new WorkflowStep(
                            new Dictionary<Status, WorkflowTransition>
                            {
                                [Status.Draft] = WorkflowTransition.Always
                            },
                            StatusColors.Published)
                },
                new List<Guid> { simpleSchemaId.Id });

            var workflows = Workflows.Empty.Set(workflow).Set(Guid.NewGuid(), simpleWorkflow);

            A.CallTo(() => appProvider.GetAppAsync(appId.Id))
                .Returns(app);

            A.CallTo(() => app.Workflows)
                .Returns(workflows);

            sut = new DynamicContentWorkflow(new JintScriptEngine(), appProvider);
        }

        [Fact]
        public async Task Should_return_draft_as_initial_status()
        {
            var result = await sut.GetInitialStatusAsync(Mocks.Schema(appId, schemaId));

            Assert.Equal(Status.Draft, result);
        }

        [Fact]
        public async Task Should_allow_publish_on_create()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanPublishOnCreateAsync(Mocks.Schema(appId, schemaId), content.Data, Mocks.FrontendUser("Editor"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_allow_publish_on_create_if_data_is_invalid()
        {
            var content = CreateContent(Status.Draft, 4);

            var result = await sut.CanPublishOnCreateAsync(Mocks.Schema(appId, schemaId), content.Data, Mocks.FrontendUser("Editor"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_not_allow_publish_on_create_if_role_not_allowed()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanPublishOnCreateAsync(Mocks.Schema(appId, schemaId), content.Data, Mocks.FrontendUser("Developer"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_check_is_valid_next()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanMoveToAsync(content, content.Status, Status.Published, Mocks.FrontendUser("Editor"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_allow_transition_if_role_is_not_allowed()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanMoveToAsync(content, content.Status, Status.Published, Mocks.FrontendUser("Developer"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_allow_transition_if_role_is_allowed()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanMoveToAsync(content, content.Status, Status.Published, Mocks.FrontendUser("Editor"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_allow_transition_if_data_not_valid()
        {
            var content = CreateContent(Status.Draft, 4);

            var result = await sut.CanMoveToAsync(content, content.Status, Status.Published, Mocks.FrontendUser("Editor"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_published()
        {
            var content = CreateContent(Status.Published, 2);

            var result = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Developer"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_draft()
        {
            var content = CreateContent(Status.Published, 2);

            var result = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Developer"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_be_able_to_update_archived()
        {
            var content = CreateContent(Status.Archived, 2);

            var result = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Developer"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_not_be_able_to_update_published_with_true_expression()
        {
            var content = CreateContent(Status.Published, 2);

            var result = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Owner"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_published_with_false_expression()
        {
            var content = CreateContent(Status.Published, 1);

            var result = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Owner"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_be_able_to_update_published_with_correct_roles()
        {
            var content = CreateContent(Status.Published, 2);

            var result = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Editor"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_published_with_incorrect_roles()
        {
            var content = CreateContent(Status.Published, 1);

            var result = await sut.CanUpdateAsync(content, content.Status, Mocks.FrontendUser("Owner"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_draft()
        {
            var content = CreateContent(Status.Draft, 2);

            var expected = new[]
            {
                new StatusInfo(Status.Archived, StatusColors.Archived)
            };

            var result = await sut.GetNextAsync(content, content.Status, Mocks.FrontendUser("Developer"));

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_limit_next_statuses_if_expression_does_not_evauate_to_true()
        {
            var content = CreateContent(Status.Draft, 4);

            var expected = new[]
            {
                new StatusInfo(Status.Archived, StatusColors.Archived)
            };

            var result = await sut.GetNextAsync(content, content.Status, Mocks.FrontendUser("Editor"));

            result.Should().BeEquivalentTo(expected);
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

            var result = await sut.GetNextAsync(content, content.Status, Mocks.FrontendUser("Editor"));

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_archived()
        {
            var content = CreateContent(Status.Archived, 2);

            var expected = new[]
            {
                new StatusInfo(Status.Draft, StatusColors.Draft)
            };

            var result = await sut.GetNextAsync(content, content.Status, null!);

            result.Should().BeEquivalentTo(expected);
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

            var result = await sut.GetNextAsync(content, content.Status, null!);

            result.Should().BeEquivalentTo(expected);
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

            var result = await sut.GetAllAsync(Mocks.Schema(appId, schemaId));

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_return_all_statuses_for_simple_schema_workflow()
        {
            var expected = new[]
            {
                new StatusInfo(Status.Draft, StatusColors.Draft),
                new StatusInfo(Status.Published, StatusColors.Published)
            };

            var result = await sut.GetAllAsync(Mocks.Schema(appId, simpleSchemaId));

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_return_all_statuses_for_default_workflow_when_no_workflow_configured()
        {
            A.CallTo(() => app.Workflows).Returns(Workflows.Empty);

            var expected = new[]
            {
                new StatusInfo(Status.Archived, StatusColors.Archived),
                new StatusInfo(Status.Draft, StatusColors.Draft),
                new StatusInfo(Status.Published, StatusColors.Published)
            };

            var result = await sut.GetAllAsync(Mocks.Schema(appId, simpleSchemaId));

            result.Should().BeEquivalentTo(expected);
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
                new NamedContentData()
                    .AddField("field",
                        new ContentFieldData()
                            .AddValue("iv", value));

            return content;
        }
    }
}