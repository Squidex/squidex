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
using Squidex.Domain.Apps.Core.Contents;
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
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");

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
        public async Task Should_only_invoke_pre_enrich_for_empty_results()
        {
            var source = Array.Empty<IContentEntity>();

            var step1 = A.Fake<IContentEnricherStep>();
            var step2 = A.Fake<IContentEnricherStep>();

            var sut = new ContentEnricher(new[] { step1, step2 }, new Lazy<IContentQueryService>(() => contentQuery));

            await sut.EnrichAsync(source, requestContext);

            A.CallTo(() => step1.EnrichAsync(requestContext))
                .MustHaveHappened();

            A.CallTo(() => step2.EnrichAsync(requestContext))
                .MustHaveHappened();

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

            await sut.EnrichAsync(source, false, requestContext);

            A.CallTo(() => step1.EnrichAsync(requestContext))
                .MustHaveHappened();

            A.CallTo(() => step2.EnrichAsync(requestContext))
                .MustHaveHappened();

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

            await sut.EnrichAsync(source, false, requestContext);

            Assert.Same(schema, step1.Schema);
            Assert.Same(schema, step1.Schema);

            A.CallTo(() => contentQuery.GetSchemaOrThrowAsync(requestContext, schemaId.Id.ToString()))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_clone_data_when_requested()
        {
            var source = CreateContent(new NamedContentData());

            var sut = new ContentEnricher(Enumerable.Empty<IContentEnricherStep>(), new Lazy<IContentQueryService>(() => contentQuery));

            var result = await sut.EnrichAsync(source, true, requestContext);

            Assert.NotSame(source.Data, result.Data);
        }

        [Fact]
        public async Task Should_not_clone_data_when_not_requested()
        {
            var source = CreateContent(new NamedContentData());

            var sut = new ContentEnricher(Enumerable.Empty<IContentEnricherStep>(), new Lazy<IContentQueryService>(() => contentQuery));

            var result = await sut.EnrichAsync(source, false, requestContext);

            Assert.Same(source.Data, result.Data);
        }

        private ContentEntity CreateContent(NamedContentData? data = null)
        {
            return new ContentEntity { SchemaId = schemaId, Data = data! };
        }
    }
}
