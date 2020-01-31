﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
    public class AssetsQueryTests : IClassFixture<AssetsQueryFixture>
    {
        public AssetsQueryFixture _ { get; }

        public AssetsQueryTests(AssetsQueryFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_find_asset_by_slug()
        {
            var asset = await _.AssetRepository.FindAssetBySlugAsync(_.RandomAppId(), _.RandomValue());

            Assert.NotNull(asset);
        }

        [Fact]
        public async Task Should_query_asset_by_hash()
        {
            var assets = await _.AssetRepository.QueryByHashAsync(_.RandomAppId(), _.RandomValue());

            Assert.NotNull(assets);
        }

        [Fact]
        public async Task Should_verify_ids()
        {
            var ids = Enumerable.Repeat(0, 50).Select(_ => Guid.NewGuid()).ToHashSet();

            var assets = await _.AssetRepository.QueryIdsAsync(_.RandomAppId(), ids);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_default(Guid? parentId)
        {
            var query = new ClrQuery();

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_tags(Guid? parentId)
        {
            var query = new ClrQuery
            {
                Filter = F.Eq("Tags", _.RandomValue())
            };

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_tags_and_name(Guid? parentId)
        {
            var query = new ClrQuery
            {
                Filter = F.And(F.Eq("Tags", _.RandomValue()), F.Contains("FileName", _.RandomValue()))
            };

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_fileName(Guid? parentId)
        {
            var query = new ClrQuery
            {
                Filter = F.Contains("FileName", _.RandomValue())
            };

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        [Theory]
        [MemberData(nameof(ParentIds))]
        public async Task Should_query_assets_by_fileName_and_tags(Guid? parentId)
        {
            var query = new ClrQuery
            {
                Filter = F.And(F.Contains("FileName", _.RandomValue()), F.Eq("Tags", _.RandomValue()))
            };

            var assets = await QueryAsync(parentId, query);

            Assert.NotNull(assets);
        }

        public static IEnumerable<object?[]> ParentIds()
        {
            yield return new object?[] { null };
            yield return new object?[] { Guid.Empty };
        }

        private async Task<IResultList<IAssetEntity>> QueryAsync(Guid? parentId, ClrQuery query)
        {
            query.Top = 1000;

            query.Skip = 100;

            query.Sort = new List<SortNode>
            {
                new SortNode("LastModified", SortOrder.Descending)
            };

            var assets = await _.AssetRepository.QueryAsync(_.RandomAppId(), parentId, query);

            return assets;
        }
    }
}
