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
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Read.Schemas.Services.Implementations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedParameter.Local

namespace Squidex.Domain.Apps.Read.Schemas
{
    public class CachingSchemaProviderTests
    {
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly Mock<ISchemaRepository> repository = new Mock<ISchemaRepository>();
        private readonly CachingSchemaProvider sut;
        private readonly ISchemaEntity schemaV1;
        private readonly ISchemaEntity schemaV2;
        private readonly NamedId<Guid> schemaId = new NamedId<Guid>(Guid.NewGuid(), "my-schema");
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");

        public CachingSchemaProviderTests()
        {
            var schemaV1Mock = new Mock<ISchemaEntity>();
            var schemaV2Mock = new Mock<ISchemaEntity>();

            schemaV1Mock.Setup(x => x.Id).Returns(schemaId.Id);
            schemaV1Mock.Setup(x => x.Name).Returns(schemaId.Name);
            schemaV1Mock.Setup(x => x.AppId).Returns(appId.Id);

            schemaV2Mock.Setup(x => x.Id).Returns(schemaId.Id);
            schemaV2Mock.Setup(x => x.Name).Returns(schemaId.Name);
            schemaV2Mock.Setup(x => x.AppId).Returns(appId.Id);

            schemaV1 = schemaV1Mock.Object;
            schemaV2 = schemaV2Mock.Object;

            sut = new CachingSchemaProvider(cache, repository.Object);
        }

        [Fact]
        public void Should_return_empty_for_events_filter()
        {
            Assert.Equal(string.Empty, sut.EventsFilter);
        }

        [Fact]
        public void Should_return_empty_for_name()
        {
            Assert.Equal(typeof(CachingSchemaProvider).Name, sut.Name);
        }

        [Fact]
        public void Should_do_nothing_when_clearing()
        {
            Assert.NotNull(sut.ClearAsync());
        }

        [Fact]
        public async Task Should_also_retrieve_schema_by_name_if_retrieved_by_id_before()
        {
            repository.Setup(x => x.FindSchemaAsync(schemaId.Id)).Returns(Task.FromResult(schemaV1));

            await ProvideSchemaById(schemaV1);
            await ProvideSchemaByName(schemaV1);

            repository.Verify(x => x.FindSchemaAsync(schemaId.Id), Times.Once());
            repository.Verify(x => x.FindSchemaAsync(appId.Id, schemaId.Name), Times.Never());
        }

        [Fact]
        public async Task Should_also_retrieve_schema_by_id_if_retrieved_by_name_before()
        {
            repository.Setup(x => x.FindSchemaAsync(appId.Id, schemaId.Name)).Returns(Task.FromResult(schemaV1));

            await ProvideSchemaByName(schemaV1);
            await ProvideSchemaById(schemaV1);

            repository.Verify(x => x.FindSchemaAsync(appId.Id, schemaId.Name), Times.Once());
            repository.Verify(x => x.FindSchemaAsync(schemaId.Id), Times.Never());
        }

        [Fact]
        public async Task Should_clear_cache_for_id_after_update_event()
        {
            var schemas = ProviderResults(schemaV1, schemaV2);

            repository.Setup(x => x.FindSchemaAsync(schemaId.Id)).Returns(() => Task.FromResult(schemas()));

            await ProvideSchemaById(schemaV1);

            sut.On(Envelope.Create(new FieldAdded { AppId = appId, SchemaId = schemaId })).Wait();

            await ProvideSchemaById(schemaV2);

            repository.Verify(x => x.FindSchemaAsync(schemaId.Id), Times.Exactly(2));
        }

        [Fact]
        public async Task Should_clear_cache_for_name_after_update_event()
        {
            var schemas = ProviderResults(schemaV1, schemaV2);

            repository.Setup(x => x.FindSchemaAsync(appId.Id, schemaId.Name)).Returns(() => Task.FromResult(schemas()));

            await ProvideSchemaByName(schemaV1);

            sut.On(Envelope.Create(new SchemaUpdated { AppId = appId, SchemaId = schemaId })).Wait();

            await ProvideSchemaByName(schemaV2);

            repository.Verify(x => x.FindSchemaAsync(appId.Id, schemaId.Name), Times.Exactly(2));
        }

        private async Task ProvideSchemaById(ISchemaEntity schema)
        {
            Assert.Equal(schema, await sut.FindSchemaByIdAsync(schemaId.Id));
        }

        private async Task ProvideSchemaByName(ISchemaEntity schema)
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
