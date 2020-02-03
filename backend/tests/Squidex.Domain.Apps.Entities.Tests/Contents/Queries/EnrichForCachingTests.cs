// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class EnrichForCachingTests
    {
        private readonly ISchemaEntity schema;
        private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
        private readonly Context requestContext;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly ProvideSchema schemaProvider;
        private readonly EnrichForCaching sut;

        public EnrichForCachingTests()
        {
            requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

            schema = Mocks.Schema(appId, schemaId);
            schemaProvider = x => Task.FromResult(schema);

            sut = new EnrichForCaching(requestCache);
        }

        [Fact]
        public async Task Should_add_app_version_and_schema_as_dependency()
        {
            var content = CreateContent();

            await sut.EnrichAsync(requestContext, Enumerable.Repeat(content, 1), schemaProvider);

            A.CallTo(() => requestCache.AddDependency(content.Id, content.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(schema.Id, schema.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(requestContext.App.Id, requestContext.App.Version))
                .MustHaveHappened();
        }

        private ContentEntity CreateContent()
        {
            return new ContentEntity { Id = Guid.NewGuid(), SchemaId = schemaId, Version = 13 };
        }
    }
}
