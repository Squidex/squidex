// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Xunit;
using F = Squidex.Infrastructure.Queries.ClrFilter;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb
{
    [Trait("Category", "Dependencies")]
    public class ContentsQueryIntegrationTests : IClassFixture<ContentsQueryFixture>
    {
        public ContentsQueryFixture _ { get; }

        public ContentsQueryIntegrationTests(ContentsQueryFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_verify_ids()
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => DomainId.NewGuid()).ToHashSet();

            var contents = await _.ContentRepository.QueryIdsAsync(_.RandomAppId(), ids, SearchScope.Published);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_by_ids()
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => DomainId.NewGuid()).ToHashSet();

            var schemas = new List<ISchemaEntity>
            {
                _.RandomSchema()
            };

            var contents = await _.ContentRepository.QueryAsync(_.RandomApp(), schemas, Q.Empty.WithIds(ids), SearchScope.All);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_by_ids_and_schema()
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => DomainId.NewGuid()).ToHashSet();

            var contents = await _.ContentRepository.QueryAsync(_.RandomApp(), _.RandomSchema(), Q.Empty.WithIds(ids), SearchScope.All);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_ids_by_filter()
        {
            var filter = F.Eq("data.field1.iv", 12);

            var contents = await _.ContentRepository.QueryIdsAsync(_.RandomAppId(), _.RandomSchemaId(), filter);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_by_filter()
        {
            var query = new ClrQuery
            {
                Filter = F.Eq("data.field1.iv", 12)
            };

            var contents = await QueryAsync(_.ContentRepository, query, 1000, 0);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_scheduled()
        {
            var time = SystemClock.Instance.GetCurrentInstant();

            await _.ContentRepository.QueryScheduledWithoutDataAsync(time).ToListAsync();
        }

        [Fact]
        public async Task Should_query_contents_with_default_query()
        {
            var query = new ClrQuery();

            var contents = await QueryAsync(_.ContentRepository, query);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_with_default_query_and_id()
        {
            var query = new ClrQuery();

            var contents = await QueryAsync(_.ContentRepository, query, reference: DomainId.NewGuid());

            Assert.Empty(contents);
        }

        [Fact]
        public async Task Should_query_contents_with_large_skip()
        {
            var query = new ClrQuery
            {
                Sort = new List<SortNode>
                {
                    new SortNode("data.value.iv", SortOrder.Ascending)
                }
            };

            var contents = await QueryAsync(_.ContentRepository, query, 1000, 9000);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_with_query_fulltext()
        {
            var query = new ClrQuery
            {
                FullText = "hello"
            };

            var contents = await QueryAsync(_.ContentRepository, query);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_with_query_filter()
        {
            var query = new ClrQuery
            {
                Filter = F.Eq("data.field1.iv", 200)
            };

            var contents = await QueryAsync(_.ContentRepository, query, 1000, 0);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_with_query_filter_and_id()
        {
            var query = new ClrQuery
            {
                Filter = F.Eq("data.value.iv", 12)
            };

            var contents = await QueryAsync(_.ContentRepository, query, 1000, 0, reference: DomainId.NewGuid());

            Assert.Empty(contents);
        }

        private async Task<IResultList<IContentEntity>> QueryAsync(IContentRepository contentRepository,
            ClrQuery clrQuery,
            int take = 1000,
            int skip = 100,
            DomainId reference = default)
        {
            if (clrQuery.Take == long.MaxValue)
            {
                clrQuery.Take = take;
            }

            if (clrQuery.Skip == 0)
            {
                clrQuery.Skip = skip;
            }

            if (clrQuery.Sort.Count == 0)
            {
                clrQuery.Sort = new List<SortNode>
                {
                    new SortNode("LastModified", SortOrder.Descending),
                    new SortNode("Id", SortOrder.Ascending)
                };
            }

            var q =
                Q.Empty
                    .WithoutTotal()
                    .WithQuery(clrQuery)
                    .WithReference(reference);

            var contents = await contentRepository.QueryAsync(_.RandomApp(), _.RandomSchema(), q, SearchScope.All);

            return contents;
        }
    }
}
