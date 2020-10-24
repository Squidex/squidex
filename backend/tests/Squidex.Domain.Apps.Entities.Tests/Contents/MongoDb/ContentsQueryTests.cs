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

            var contents = await _.ContentRepository.QueryAsync(_.RandomApp(), ids, SearchScope.All);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_by_ids_and_schema()
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => DomainId.NewGuid()).ToHashSet();

            var contents = await _.ContentRepository.QueryAsync(_.RandomApp(), _.RandomSchema(), ids, SearchScope.All);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_ids_by_filter()
        {
            var filter = F.Eq("data.value.iv", 12);

            var contents = await _.ContentRepository.QueryIdsAsync(_.RandomAppId(), _.RandomSchemaId(), filter);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_by_filter()
        {
            var query = new ClrQuery
            {
                Sort = new List<SortNode>
                {
                    new SortNode("lastModified", SortOrder.Descending)
                },
                Filter = F.Eq("data.value.iv", 12)
            };

            var contents = await _.ContentRepository.QueryAsync(_.RandomApp(), _.RandomSchema(), query, null, SearchScope.Published);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_scheduled()
        {
            var time = SystemClock.Instance.GetCurrentInstant();

            await _.ContentRepository.QueryScheduledWithoutDataAsync(time, _ => Task.CompletedTask);
        }

        [Fact]
        public async Task Should_query_contents_with_default_query()
        {
            var query = new ClrQuery();

            var contents = await QueryAsync(query);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_with_default_query_and_id()
        {
            var query = new ClrQuery();

            var contents = await QueryAsync(query, id: DomainId.NewGuid());

            Assert.NotEmpty(contents);
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

            var contents = await QueryAsync(query, 1000, 9000);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_with_query_fulltext()
        {
            var query = new ClrQuery
            {
                FullText = "hello"
            };

            var contents = await QueryAsync(query);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_with_query_filter()
        {
            var query = new ClrQuery
            {
                Filter = F.Eq("data.value.iv", 200)
            };

            var contents = await QueryAsync(query, 1000, 0);

            Assert.NotEmpty(contents);
        }

        [Fact]
        public async Task Should_query_contents_with_query_filter_and_id()
        {
            var query = new ClrQuery
            {
                Filter = F.Eq("data.value.iv", 12)
            };

            var contents = await QueryAsync(query, 1000, 0, id: DomainId.NewGuid());

            Assert.Empty(contents);
        }

        private async Task<IResultList<IContentEntity>> QueryAsync(ClrQuery clrQuery, int take = 1000, int skip = 100, DomainId? id = null)
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

            var contents = await _.ContentRepository.QueryAsync(_.RandomApp(), _.RandomSchema(), clrQuery, id, SearchScope.All);

            return contents;
        }
    }
}
