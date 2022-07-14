// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans.Core;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public class SchemasCacheGrainTests
    {
        private readonly IGrainIdentity identity = A.Fake<IGrainIdentity>();
        private readonly ISchemaRepository schemaRepository = A.Fake<ISchemaRepository>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly SchemasCacheGrain sut;

        public SchemasCacheGrainTests()
        {
            A.CallTo(() => identity.PrimaryKeyString)
                .Returns(appId.ToString());

            sut = new SchemasCacheGrain(identity, schemaRepository);
        }

        [Fact]
        public async Task Should_not_reserve_name_if_already_used()
        {
            var ids = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid()
            };

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .Returns(ids);

            var token1 = await sut.ReserveAsync(DomainId.NewGuid(), "name1");
            var token2 = await sut.ReserveAsync(DomainId.NewGuid(), "name2");

            Assert.Null(token1);
            Assert.NotNull(token2);
        }

        [Fact]
        public async Task Should_provide_schema_ids_from_repository_once()
        {
            var ids = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid(),
                ["name2"] = DomainId.NewGuid()
            };

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .Returns(ids);

            var result1 = await sut.GetSchemaIdsAsync();
            var result2 = await sut.GetSchemaIdsAsync();

            Assert.Equal(ids.Values, result1);
            Assert.Equal(ids.Values, result2);

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_add_id_to_loaded_result()
        {
            var ids = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid(),
                ["name2"] = DomainId.NewGuid()
            };

            var newId = DomainId.NewGuid();

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .Returns(ids);

            await sut.GetSchemaIdsAsync();
            await sut.AddAsync(newId, "new-name");

            var result = await sut.GetSchemaIdsAsync();

            Assert.Equal(ids.Values.Union(Enumerable.Repeat(newId, 1)), result);

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_remove_id_from_loaded_result()
        {
            var ids = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid(),
                ["name2"] = DomainId.NewGuid()
            };

            var newId = DomainId.NewGuid();

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .Returns(ids);

            await sut.GetSchemaIdsAsync();
            await sut.RemoveAsync(ids.ElementAt(0).Value);

            var result = await sut.GetSchemaIdsAsync();

            Assert.Equal(ids.Values.Skip(1), result);

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_remove_id_from_not_loaded_result()
        {
            var ids = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid(),
                ["name2"] = DomainId.NewGuid()
            };

            var newId = DomainId.NewGuid();

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .Returns(ids);

            await sut.RemoveAsync(ids.ElementAt(0).Value);

            var result = await sut.GetSchemaIdsAsync();

            Assert.Equal(ids.Values.Skip(1), result);

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_merge_found_value_with_added_id()
        {
            var foundId = DomainId.NewGuid();

            async Task<Dictionary<string, DomainId>> GetIds()
            {
                await sut.AddAsync(foundId, "name1");

                return new Dictionary<string, DomainId>();
            }

            A.CallTo(() => schemaRepository.QueryIdsAsync(appId, default))
                .ReturnsLazily(() => GetIds());

            var result1 = await sut.GetSchemaIdsAsync();
            var result2 = await sut.GetSchemaIdsAsync();

            Assert.Equal(foundId, result1.Single());
            Assert.Equal(foundId, result2.Single());
        }
    }
}
