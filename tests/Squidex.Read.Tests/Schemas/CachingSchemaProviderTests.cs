// ==========================================================================
//  CachingSchemaProviderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Squidex.Infrastructure;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.Schemas.Services.Implementations;
using Squidex.Read.MongoDb.Schemas;
using Xunit;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedParameter.Local

namespace Squidex.Read.Schemas
{
    public class CachingSchemaProviderTests
    {
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly Mock<ISchemaRepository> repository = new Mock<ISchemaRepository>();
        private readonly CachingSchemaProvider sut;
        private readonly MongoSchemaEntity schemaV1;
        private readonly MongoSchemaEntity schemaV2;
        private readonly NamedId<Guid> schemaId = new NamedId<Guid>(Guid.NewGuid(), "my-schema");
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");

        public CachingSchemaProviderTests()
        {
            schemaV1 = new MongoSchemaEntity { Name = schemaId.Name, Id = schemaId.Id, AppId = appId.Id };
            schemaV2 = new MongoSchemaEntity { Name = schemaId.Name, Id = schemaId.Id, AppId = appId.Id };

            sut = new CachingSchemaProvider(cache, repository.Object);
        }

        [Fact]
        public async Task Should_also_retrieve_schema_by_name_if_retrieved_by_id_before()
        {
            repository.Setup(x => x.FindSchemaAsync(schemaId.Id)).Returns(Task.FromResult<ISchemaEntityWithSchema>(schemaV1));

            await ProvideSchemaById(schemaV1);
            await ProvideSchemaByName(schemaV1);

            repository.Verify(x => x.FindSchemaAsync(schemaId.Id), Times.Once());
            repository.Verify(x => x.FindSchemaAsync(appId.Id, schemaId.Name), Times.Never());
        }

        [Fact]
        public async Task Should_also_retrieve_schema_by_id_if_retrieved_by_name_before()
        {
            repository.Setup(x => x.FindSchemaAsync(appId.Id, schemaId.Name)).Returns(Task.FromResult<ISchemaEntityWithSchema>(schemaV1));

            await ProvideSchemaByName(schemaV1);
            await ProvideSchemaById(schemaV1);

            repository.Verify(x => x.FindSchemaAsync(appId.Id, schemaId.Name), Times.Once());
            repository.Verify(x => x.FindSchemaAsync(schemaId.Id), Times.Never());
        }

        [Theory]
        public async Task Should_clear_cache_for_id_after_update_event()
        {
            var schemas = ProviderResults(schemaV1, schemaV2);

            repository.Setup(x => x.FindSchemaAsync(schemaId.Id)).Returns(() => Task.FromResult<ISchemaEntityWithSchema>(schemas()));

            await ProvideSchemaById(schemaV1);

            sut.Remove(appId, schemaId);

            await ProvideSchemaById(schemaV2);

            repository.Verify(x => x.FindSchemaAsync(schemaId.Id), Times.Exactly(2));
        }

        [Theory]
        public async Task Should_clear_cache_for_name_after_update_event()
        {
            var schemas = ProviderResults(schemaV1, schemaV2);

            repository.Setup(x => x.FindSchemaAsync(appId.Id, schemaId.Name)).Returns(() => Task.FromResult<ISchemaEntityWithSchema>(schemas()));

            await ProvideSchemaByName(schemaV1);

            sut.Remove(appId, schemaId);

            await ProvideSchemaByName(schemaV2);

            repository.Verify(x => x.FindSchemaAsync(appId.Id, schemaId.Name), Times.Exactly(2));
        }

        private async Task ProvideSchemaById(ISchemaEntityWithSchema schema)
        {
            Assert.Equal(schema, await sut.FindSchemaByIdAsync(schemaId.Id));
        }

        private async Task ProvideSchemaByName(ISchemaEntityWithSchema schema)
        {
            Assert.Equal(schema, await sut.FindSchemaByNameAsync(appId.Id, schemaId.Name));
        }

        private static Func<T> ProviderResults<T>(params T[] items)
        {
            var index = 0;

            return () => items[index++];
        }
    }
}
