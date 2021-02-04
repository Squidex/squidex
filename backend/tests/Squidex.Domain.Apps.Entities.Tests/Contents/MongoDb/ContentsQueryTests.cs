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
    public class ContentsQueryTests : IClassFixture<ContentsQueryFixture>
    {
        public ContentsQueryFixture _ { get; }

        public ContentsQueryTests(ContentsQueryFixture fixture)
        {
            _ = fixture;
        }

        public IEnumerable<object[]> Collections()
        {
            yield return new[] { _.ContentRepository };
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_verify_ids(IContentRepository repository)
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => DomainId.NewGuid()).ToHashSet();

            var contents = await repository.QueryIdsAsync(_.RandomAppId(), ids, SearchScope.Published);

            Assert.NotNull(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_by_ids(IContentRepository repository)
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => DomainId.NewGuid()).ToHashSet();

            var schemas = new List<ISchemaEntity>
            {
                _.RandomSchema()
            };

            var contents = await repository.QueryAsync(_.RandomApp(), schemas, Q.Empty.WithIds(ids), SearchScope.All);

            Assert.NotNull(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_by_ids_and_schema(IContentRepository repository)
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => DomainId.NewGuid()).ToHashSet();

            var contents = await repository.QueryAsync(_.RandomApp(), _.RandomSchema(), Q.Empty.WithIds(ids), SearchScope.All);

            Assert.NotNull(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_ids_by_filter(IContentRepository repository)
        {
            var filter = F.Eq("data.value.iv", 12);

            var contents = await repository.QueryIdsAsync(_.RandomAppId(), _.RandomSchemaId(), filter);

            Assert.NotEmpty(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_by_filter(IContentRepository repository)
        {
            var query = new ClrQuery
            {
                Sort = new List<SortNode>
                {
                    new SortNode("lastModified", SortOrder.Descending)
                },
                Filter = F.Eq("data.value.iv", 12)
            };

            var contents = await repository.QueryAsync(_.RandomApp(), _.RandomSchema(), Q.Empty.WithQuery(query), SearchScope.Published);

            Assert.NotEmpty(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_scheduled(IContentRepository repository)
        {
            var time = SystemClock.Instance.GetCurrentInstant();

            await repository.QueryScheduledWithoutDataAsync(time, _ => Task.CompletedTask);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_with_default_query(IContentRepository repository)
        {
            var query = new ClrQuery();

            var contents = await QueryAsync(repository, query);

            Assert.NotEmpty(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_with_default_query_and_id(IContentRepository repository)
        {
            var query = new ClrQuery();

            var contents = await QueryAsync(repository, query, reference: DomainId.NewGuid());

            Assert.Empty(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_with_large_skip(IContentRepository repository)
        {
            var query = new ClrQuery
            {
                Sort = new List<SortNode>
                {
                    new SortNode("data.value.iv", SortOrder.Ascending)
                }
            };

            var contents = await QueryAsync(repository, query, 1000, 9000);

            Assert.NotEmpty(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_with_query_fulltext(IContentRepository repository)
        {
            var query = new ClrQuery
            {
                FullText = "hello"
            };

            var contents = await QueryAsync(repository, query);

            Assert.NotNull(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_with_query_filter(IContentRepository repository)
        {
            var query = new ClrQuery
            {
                Filter = F.Eq("data.value.iv", 200)
            };

            var contents = await QueryAsync(repository, query, 1000, 0);

            Assert.NotEmpty(contents);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async Task Should_query_contents_with_query_filter_and_id(IContentRepository repository)
        {
            var query = new ClrQuery
            {
                Filter = F.Eq("data.value.iv", 12)
            };

            var contents = await QueryAsync(repository, query, 1000, 0, reference: DomainId.NewGuid());

            Assert.Empty(contents);
        }

        private async Task<IResultList<IContentEntity>> QueryAsync(IContentRepository repository,
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
                    new SortNode("LastModified", SortOrder.Descending)
                };
            }

            var q =
                Q.Empty
                    .WithQuery(clrQuery)
                    .WithReference(reference);

            var contents = await repository.QueryAsync(_.RandomApp(), _.RandomSchema(), q, SearchScope.All);

            return contents;
        }
    }
}
