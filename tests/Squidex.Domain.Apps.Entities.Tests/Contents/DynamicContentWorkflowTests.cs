// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class DynamicContentWorkflowTests
    {
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAppEntity appEntity = A.Fake<IAppEntity>();
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
                            [Status.Published] = new WorkflowTransition("data.field.iv === 2", "Editor")
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
            A.CallTo(() => appProvider.GetAppAsync(appId.Id))
                .Returns(appEntity);

            A.CallTo(() => appEntity.Workflows)
                .Returns(Workflows.Empty.Set(workflow));

            sut = new DynamicContentWorkflow(new JintScriptEngine(), appProvider);
        }

        [Fact]
        public async Task Should_draft_as_initial_status()
        {
            var expected = new StatusInfo(Status.Draft, StatusColors.Draft);

            var result = await sut.GetInitialStatusAsync(CreateSchema());

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_check_is_valid_next()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanMoveToAsync(content, Status.Published, User("Editor"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_allow_transition_if_role_is_not_allowed()
        {
            var content = CreateContent(Status.Draft, 2);

            var result = await sut.CanMoveToAsync(content, Status.Published, User("Developer"));

            Assert.False(result);
        }

        [Fact]
        public async Task Should_not_allow_transition_if_expression_does_not_evauate_to_true()
        {
            var content = CreateContent(Status.Draft, 4);

            var result = await sut.CanMoveToAsync(content, Status.Published, User("Editor"));

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

            var result = await sut.GetNextsAsync(content, User("Developer"));

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

            var result = await sut.GetNextsAsync(content, User("Editor"));

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

            var result = await sut.GetNextsAsync(content, User("Editor"));

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

            var result = await sut.GetAllAsync(CreateSchema());

            result.Should().BeEquivalentTo(expected);
        }

        private ISchemaEntity CreateSchema()
        {
            var schema = A.Fake<ISchemaEntity>();

            A.CallTo(() => schema.AppId).Returns(appId);

            return schema;
        }

        private IContentEntity CreateContent(Status status, int value)
        {
            var data =
                new NamedContentData()
                    .AddField("field",
                        new ContentFieldData()
                            .AddValue("iv", value));

            return new ContentEntity { AppId = appId, Status = status, DataDraft = data };
        }

        private ClaimsPrincipal User(string role)
        {
            var userIdentity = new ClaimsIdentity();
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            userIdentity.AddClaim(new Claim(ClaimTypes.Role, role));

            return userPrincipal;
        }
    }
}
