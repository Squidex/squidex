// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentEnricherTests
    {
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly ISchemaEntity schema;
        private readonly Context requestContext;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");

        private sealed class ResolveSchema : IContentEnricherStep
        {
            public ISchemaEntity Schema { get; private set; }

            public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
            {
                foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
                {
                    Schema = await schemas(group.Key);
                }
            }
        }

        public ContentEnricherTests()
        {
            requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

            schema = Mocks.Schema(appId, schemaId);

            A.CallTo(() => contentQuery.GetSchemaOrThrowAsync(requestContext, schemaId.Id.ToString()))
                .Returns(schema);
        }

        [Fact]
        public async Task Should_not_invoke_steps()
        {
            var source = new IContentEntity[0];

            var step1 = A.Fake<IContentEnricherStep>();
            var step2 = A.Fake<IContentEnricherStep>();

            var sut = new ContentEnricher(new[] { step1, step2 }, new Lazy<IContentQueryService>(() => contentQuery));

            await sut.EnrichAsync(source, requestContext);

            A.CallTo(() => step1.EnrichAsync(requestContext, A<IEnumerable<ContentEntity>>._, A<ProvideSchema>._))
                .MustNotHaveHappened();

            A.CallTo(() => step2.EnrichAsync(requestContext, A<IEnumerable<ContentEntity>>._, A<ProvideSchema>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_steps()
        {
            var source = CreateContent();

            var step1 = A.Fake<IContentEnricherStep>();
            var step2 = A.Fake<IContentEnricherStep>();

            var sut = new ContentEnricher(new[] { step1, step2 }, new Lazy<IContentQueryService>(() => contentQuery));

            await sut.EnrichAsync(source, requestContext);

            A.CallTo(() => step1.EnrichAsync(requestContext, A<IEnumerable<ContentEntity>>._, A<ProvideSchema>._))
                .MustHaveHappened();

            A.CallTo(() => step2.EnrichAsync(requestContext, A<IEnumerable<ContentEntity>>._, A<ProvideSchema>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_provide_and_cache_schema()
        {
            var source = CreateContent();

            var step1 = new ResolveSchema();
            var step2 = new ResolveSchema();

            var sut = new ContentEnricher(new[] { step1, step2 }, new Lazy<IContentQueryService>(() => contentQuery));

            await sut.EnrichAsync(source, requestContext);

            Assert.Same(schema, step1.Schema);
            Assert.Same(schema, step1.Schema);

            A.CallTo(() => contentQuery.GetSchemaOrThrowAsync(requestContext, schemaId.Id.ToString()))
                .MustHaveHappenedOnceExactly();
        }

        private ContentEntity CreateContent()
        {
            return new ContentEntity { SchemaId = schemaId };
        }
    }
}
