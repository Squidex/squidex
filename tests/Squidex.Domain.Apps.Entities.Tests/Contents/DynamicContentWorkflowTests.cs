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
                            [Status.Draft] = new WorkflowTransition()
                        },
                        StatusColors.Archived, true),
                [Status.Draft] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = new WorkflowTransition(),
                            [Status.Published] = new WorkflowTransition("data.field.iv === 2", new[] { "Editor" })
                        },
                        StatusColors.Draft),
                [Status.Published] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = new WorkflowTransition(),
                            [Status.Draft] = new WorkflowTransition()
                        },
                        StatusColors.Published)
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
                                [Status.Published] = new WorkflowTransition()
                            },
                            StatusColors.Draft),
                    [Status.Published] =
                        new WorkflowStep(
                            new Dictionary<Status, WorkflowTransition>
                            {
                                [Status.Draft] = new WorkflowTransition()
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
        public async Task Should_draft_as_initial_status()
        {
            var expected = new StatusInfo(Status.Draft, StatusColors.Draft);

            var result = await sut.GetInitialStatusAsync(Mocks.Schema(appId, schemaId));

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_allow_publish_on_create()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanPublishOnCreateAsync(Mocks.Schema(appId, schemaId), content.DataDraft, Mocks.FrontendUser("Editor"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_allow_publish_on_create_if_data_is_invalid()
        {
            var content = CreateContent(Status.Draft, 4);

            var result = await sut.CanPublishOnCreateAsync(Mocks.Schema(appId, schemaId), content.DataDraft, Mocks.FrontendUser("Editor"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_not_allow_publish_on_create_if_role_not_allowed()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanPublishOnCreateAsync(Mocks.Schema(appId, schemaId), content.DataDraft, Mocks.FrontendUser("Developer"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_check_is_valid_next()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanMoveToAsync(content, Status.Published, Mocks.FrontendUser("Editor"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_allow_transition_if_role_is_not_allowed()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanMoveToAsync(content, Status.Published, Mocks.FrontendUser("Developer"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_not_allow_transition_if_data_not_valid()
        {
            var content = CreateContent(Status.Draft, 4);

            var result = await sut.CanMoveToAsync(content, Status.Published, Mocks.FrontendUser("Editor"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_published()
        {
            var content = CreateContent(Status.Published, 2);

            var result = await sut.CanUpdateAsync(content);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_draft()
        {
            var content = CreateContent(Status.Published, 2);

            var result = await sut.CanUpdateAsync(content);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_be_able_to_update_archived()
        {
            var content = CreateContent(Status.Archived, 2);

            var result = await sut.CanUpdateAsync(content);

            Assert.False(result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_draft()
        {
            var content = CreateContent(Status.Draft, 2);

            var expected = new[]
            {
                new StatusInfo(Status.Archived, StatusColors.Archived)
            };

            var result = await sut.GetNextsAsync(content, Mocks.FrontendUser("Developer"));

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

            var result = await sut.GetNextsAsync(content, Mocks.FrontendUser("Editor"));

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

            var result = await sut.GetNextsAsync(content, Mocks.FrontendUser("Editor"));

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

            var result = await sut.GetNextsAsync(content, null);

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

            var result = await sut.GetNextsAsync(content, null);

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

        private IContentEntity CreateContent(Status status, int value, bool simple = false)
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

            content.DataDraft =
                new NamedContentData()
                    .AddField("field",
                        new ContentFieldData()
                            .AddValue("iv", value));

            return content;
        }
    }
}
