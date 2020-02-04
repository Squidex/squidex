// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class EnrichWithSchemaTests
    {
        private readonly ISchemaEntity schema;
        private readonly Context requestContext;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly ProvideSchema schemaProvider;
        private readonly EnrichWithSchema sut;

        public EnrichWithSchemaTests()
        {
            requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

            schema = Mocks.Schema(appId, schemaId);
            schemaProvider = x => Task.FromResult(schema);

            sut = new EnrichWithSchema();
        }

        [Fact]
        public async Task Should_enrich_with_reference_fields()
        {
            var content = CreateContent();

            var ctx = new Context(Mocks.FrontendUser(), requestContext.App);

            await sut.EnrichAsync(ctx, Enumerable.Repeat(content, 1), schemaProvider);

            Assert.NotNull(content.ReferenceFields);
        }

        [Fact]
        public async Task Should_not_enrich_with_reference_fields_when_not_frontend()
        {
            var source = CreateContent();

            await sut.EnrichAsync(requestContext, Enumerable.Repeat(source, 1), schemaProvider);

            Assert.Null(source.ReferenceFields);
        }

        [Fact]
        public async Task Should_enrich_with_schema_names()
        {
            var content = CreateContent();

            await sut.EnrichAsync(requestContext, Enumerable.Repeat(content, 1), schemaProvider);

            Assert.Equal("my-schema", content.SchemaName);
            Assert.Equal("my-schema", content.SchemaDisplayName);
        }

        private ContentEntity CreateContent()
        {
            return new ContentEntity { SchemaId = schemaId };
        }
    }
}
