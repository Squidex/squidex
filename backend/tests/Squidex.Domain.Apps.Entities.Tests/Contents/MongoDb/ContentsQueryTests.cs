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
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Tasks;
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
            var ids = Enumerable.Repeat(0, 50).Select(_ => Guid.NewGuid()).ToHashSet();

            var contents = await _.ContentRepository.QueryIdsAsync(_.RandomAppId(), ids, SearchScope.Published);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_by_ids()
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => Guid.NewGuid()).ToHashSet();

            var contents = await _.ContentRepository.QueryAsync(_.RandomApp(), new[] { Status.Published }, ids, SearchScope.All);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_by_ids_and_schema()
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => Guid.NewGuid()).ToHashSet();

            var contents = await _.ContentRepository.QueryAsync(_.RandomApp(), _.RandomSchema(), new[] { Status.Published }, ids, SearchScope.All);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_by_filter()
        {
            var filter = F.Eq("data.value.iv", _.RandomValue());

            var contents = await _.ContentRepository.QueryIdsAsync(_.RandomAppId(), _.RandomSchemaId(), filter);

            Assert.NotNull(contents);
        }

        [Fact]
        public async Task Should_query_contents_scheduled()
        {
            var time = SystemClock.Instance.GetCurrentInstant();

            await _.ContentRepository.QueryScheduledWithoutDataAsync(time, _ => TaskHelper.Done);
        }

        [Theory]
        [MemberData(nameof(Statuses))]
        public async Task Should_query_contents_by_default(Status[]? status)
        {
            var query = new ClrQuery();

            var contents = await QueryAsync(status, query);

            Assert.NotNull(contents);
        }

        [Theory]
        [MemberData(nameof(Statuses))]
        public async Task Should_query_contents_with_query_fulltext(Status[]? status)
        {
            var query = new ClrQuery
            {
                FullText = "hello"
            };

            var contents = await QueryAsync(status, query);

            Assert.NotNull(contents);
        }

        [Theory]
        [MemberData(nameof(Statuses))]
        public async Task Should_query_contents_with_query_filter(Status[]? status)
        {
            var query = new ClrQuery
            {
                Filter = F.Eq("data.value.iv", _.RandomValue())
            };

            var contents = await QueryAsync(status, query);

            Assert.NotNull(contents);
        }

        public static IEnumerable<object?[]> Statuses()
        {
            yield return new object?[] { null };
            yield return new object?[] { new[] { Status.Published } };
        }

        private async Task<IResultList<IContentEntity>> QueryAsync(Status[]? status, ClrQuery clrQuery)
        {
            clrQuery.Top = 1000;
            clrQuery.Skip = 100;
            clrQuery.Sort = new List<SortNode> { new SortNode("LastModified", SortOrder.Descending) };

            var contents = await _.ContentRepository.QueryAsync(_.RandomApp(), _.RandomSchema(), status, clrQuery, SearchScope.All);

            return contents;
        }
    }
}
