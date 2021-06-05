// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Xunit;
using F = Squidex.Infrastructure.Queries.ClrFilter;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb
{
    [Trait("Category", "Dependencies")]
    public class AssetsQueryIntegrationTests : IClassFixture<AssetsQueryFixture>
    {
        public AssetsQueryFixture _ { get; }

        public AssetsQueryIntegrationTests(AssetsQueryFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_find_asset_by_slug()
        {
            var random = _.RandomValue();

            var asset = await _.AssetRepository.FindAssetBySlugAsync(_.RandomAppId(), random);

            Assert.NotNull(asset);
        }

        [Fact]
        public async Task Should_query_asset_by_hash()
        {
            var random = _.RandomValue();

            var assets = await _.AssetRepository.FindAssetByHashAsync(_.RandomAppId(), random, random, 1024);

            Assert.NotNull(assets);
        }

        [Fact]
        public async Task Should_verify_ids()
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => DomainId.NewGuid()).ToHashSet();

            var assets = await _.AssetRepository.QueryIdsAsync(_.RandomAppId(), ids);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_default(DomainId? parentId)
        {
            var query = new ClrQuery();

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_tags(DomainId? parentId)
        {
            var random = _.RandomValue();

            var query = new ClrQuery
            {
                Filter = F.Eq("Tags", random)
            };

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_tags_and_name(DomainId? parentId)
        {
            var random = _.RandomValue();

            var query = new ClrQuery
            {
                Filter = F.And(F.Eq("Tags", random), F.Contains("FileName", random))
            };

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_fileName(DomainId? parentId)
        {
            var random = _.RandomValue();

            var query = new ClrQuery
            {
                Filter = F.Contains("FileName", random)
            };

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_fileName_and_tags(DomainId? parentId)
        {
            var random = _.RandomValue();

            var query = new ClrQuery
            {
                Filter = F.And(F.Contains("FileName", random), F.Eq("Tags", random))
            };

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        public static IEnumerable<object?[]> ParentIds()
        {
            yield return new object?[] { null };
            yield return new object?[] { DomainId.Empty };
        }

        private async Task<IResultList<IAssetEntity>> QueryAsync(DomainId? parentId, ClrQuery clrQuery)
        {
            clrQuery.Top = 1000;

            clrQuery.Skip = 100;

            if (clrQuery.Sort.Count == 0)
            {
                clrQuery.Sort = new List<SortNode>
                {
                    new SortNode("LastModified", SortOrder.Descending),
                    new SortNode("Id", SortOrder.Descending)
                };
            }

            var q =
                Q.Empty
                    .WithoutTotal()
                    .WithQuery(clrQuery);

            var assets = await _.AssetRepository.QueryAsync(_.RandomAppId(), parentId, q);

            return assets;
        }
    }
}
