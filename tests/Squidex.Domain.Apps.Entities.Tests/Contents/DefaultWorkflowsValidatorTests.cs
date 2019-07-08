// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class DefaultWorkflowsValidatorTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly DefaultWorkflowsValidator sut;

        public DefaultWorkflowsValidatorTests()
        {
            var schema = A.Fake<ISchemaEntity>();

            A.CallTo(() => schema.Id).Returns(schemaId.Id);
            A.CallTo(() => schema.SchemaDef).Returns(new Schema(schemaId.Name));

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<Guid>.Ignored, false))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns(schema);

            sut = new DefaultWorkflowsValidator(appProvider);
        }

        [Fact]
        public async Task Should_generate_error_if_multiple_workflows_cover_all_schemas()
        {
            var workflows = Workflows.Empty
                .Add(Guid.NewGuid(), "workflow1")
                .Add(Guid.NewGuid(), "workflow2");

            var errors = await sut.ValidateAsync(appId.Id, workflows);

            Assert.Equal(errors, new string[] { "Multiple workflows cover all schemas." });
        }

        [Fact]
        public async Task Should_generate_error_if_multiple_workflows_cover_specific_schema()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var workflows = Workflows.Empty
                .Add(id1, "workflow1")
                .Add(id2, "workflow2")
                .Update(id1, new Workflow(default, Workflow.EmptySteps, new List<Guid> { schemaId.Id }))
                .Update(id2, new Workflow(default, Workflow.EmptySteps, new List<Guid> { schemaId.Id }));

            var errors = await sut.ValidateAsync(appId.Id, workflows);

            Assert.Equal(errors, new string[] { "The schema `my-schema` is covered by multiple workflows." });
        }

        [Fact]
        public async Task Should_not_generate_error_if_schema_deleted()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var oldSchemaId = Guid.NewGuid();

            var workflows = Workflows.Empty
                .Add(id1, "workflow1")
                .Add(id2, "workflow2")
                .Update(id1, new Workflow(default, Workflow.EmptySteps, new List<Guid> { oldSchemaId }))
                .Update(id2, new Workflow(default, Workflow.EmptySteps, new List<Guid> { oldSchemaId }));

            var errors = await sut.ValidateAsync(appId.Id, workflows);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_generate_errors_for_no_overlaps()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var workflows = Workflows.Empty
                .Add(id1, "workflow1")
                .Add(id2, "workflow2")
                .Update(id1, new Workflow(default, Workflow.EmptySteps, new List<Guid> { schemaId.Id }));

            var errors = await sut.ValidateAsync(appId.Id, workflows);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_generate_errors_for_empty_workflows()
        {
            var errors = await sut.ValidateAsync(appId.Id, Workflows.Empty);

            Assert.Empty(errors);
        }
    }
}
