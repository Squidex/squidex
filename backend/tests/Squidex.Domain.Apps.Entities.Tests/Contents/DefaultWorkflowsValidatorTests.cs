// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class DefaultWorkflowsValidatorTests : IClassFixture<TranslationsFixture>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly DefaultWorkflowsValidator sut;

        public DefaultWorkflowsValidatorTests()
        {
            var schema = Mocks.Schema(appId, schemaId, new Schema(schemaId.Name));

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<DomainId>._, false, default))
                .Returns(Task.FromResult<ISchemaEntity?>(null));

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false, default))
                .Returns(schema);

            sut = new DefaultWorkflowsValidator(appProvider);
        }

        [Fact]
        public async Task Should_generate_error_if_multiple_workflows_cover_all_schemas()
        {
            var workflows = Workflows.Empty
                .Add(DomainId.NewGuid(), "workflow1")
                .Add(DomainId.NewGuid(), "workflow2");

            var errors = await sut.ValidateAsync(appId.Id, workflows);

            Assert.Equal(new[] { "Multiple workflows cover all schemas." }, errors.ToArray());
        }

        [Fact]
        public async Task Should_generate_error_if_multiple_workflows_cover_specific_schema()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var workflows = Workflows.Empty
                .Add(id1, "workflow1")
                .Add(id2, "workflow2")
                .Update(id1, new Workflow(default, null, ImmutableList.Create(schemaId.Id)))
                .Update(id2, new Workflow(default, null, ImmutableList.Create(schemaId.Id)));

            var errors = await sut.ValidateAsync(appId.Id, workflows);

            Assert.Equal(new[] { "The schema 'my-schema' is covered by multiple workflows." }, errors.ToArray());
        }

        [Fact]
        public async Task Should_not_generate_error_if_schema_deleted()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var oldSchemaId = DomainId.NewGuid();

            var workflows = Workflows.Empty
                .Add(id1, "workflow1")
                .Add(id2, "workflow2")
                .Update(id1, new Workflow(default, null, ImmutableList.Create(oldSchemaId)))
                .Update(id2, new Workflow(default, null, ImmutableList.Create(oldSchemaId)));

            var errors = await sut.ValidateAsync(appId.Id, workflows);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_generate_errors_for_no_overlaps()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var workflows = Workflows.Empty
                .Add(id1, "workflow1")
                .Add(id2, "workflow2")
                .Update(id1, new Workflow(default, null, ImmutableList.Create(schemaId.Id)));

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
