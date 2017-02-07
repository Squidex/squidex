// ==========================================================================
//  CachingSchemaProviderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Squidex.Events;
using Squidex.Events.Schemas;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
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
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly Guid appId = Guid.NewGuid();
        private readonly string schemaName = "my-schema";

        private sealed class MyEvent : IEvent
        {
        }

        public CachingSchemaProviderTests()
        {
            schemaV1 = new MongoSchemaEntity { Name = schemaName, Id = schemaId, AppId = appId };
            schemaV2 = new MongoSchemaEntity { Name = schemaName, Id = schemaId, AppId = appId };

            sut = new CachingSchemaProvider(cache, repository.Object);
        }

        [Fact]
        public async Task Should_also_retrieve_schema_by_name_if_retrieved_by_id_before()
        {
            repository.Setup(x => x.FindSchemaAsync(schemaId)).Returns(Task.FromResult<ISchemaEntityWithSchema>(schemaV1));

            await ProvideSchemaById(schemaV1);
            await ProvideSchemaByName(schemaV1);

            repository.Verify(x => x.FindSchemaAsync(schemaId), Times.Once());
            repository.Verify(x => x.FindSchemaAsync(appId, schemaName), Times.Never());
        }

        [Fact]
        public async Task Should_also_retrieve_schema_by_id_if_retrieved_by_name_before()
        {
            repository.Setup(x => x.FindSchemaAsync(appId, schemaName)).Returns(Task.FromResult<ISchemaEntityWithSchema>(schemaV1));

            await ProvideSchemaByName(schemaV1);
            await ProvideSchemaById(schemaV1);

            repository.Verify(x => x.FindSchemaAsync(appId, schemaName), Times.Once());
            repository.Verify(x => x.FindSchemaAsync(schemaId), Times.Never());
        }

        [Fact]
        public async Task Should_ignore_other_events()
        {
            repository.Setup(x => x.FindSchemaAsync(schemaId)).Returns(Task.FromResult<ISchemaEntityWithSchema>(schemaV1));

            await ProvideSchemaById(schemaV1);
            await RaiseEvent(new MyEvent());
            await ProvideSchemaById(schemaV1);

            repository.Verify(x => x.FindSchemaAsync(schemaId), Times.Once());
        }

        [Fact]
        public async Task Should_retrieve_by_id_after_created_event()
        {
            var schemas = ProviderResults(null, schemaV1);

            repository.Setup(x => x.FindSchemaAsync(schemaId)).Returns(() => Task.FromResult<ISchemaEntityWithSchema>(schemas()));

            await ProvideSchemaById(null);
            await RaiseEvent(new SchemaCreated { Name = schemaName });
            await ProvideSchemaById(schemaV1);

            repository.Verify(x => x.FindSchemaAsync(schemaId), Times.Exactly(2));
        }

        [Fact]
        public async Task Should_retrieve_by_name_after_created_event()
        {
            var schemas = ProviderResults(null, schemaV1);

            repository.Setup(x => x.FindSchemaAsync(appId, schemaName)).Returns(() => Task.FromResult<ISchemaEntityWithSchema>(schemas()));

            await ProvideSchemaByName(null);
            await RaiseEvent(new SchemaCreated { Name = schemaName });
            await ProvideSchemaByName(schemaV1);

            repository.Verify(x => x.FindSchemaAsync(appId, schemaName), Times.Exactly(2));
        }

        [Theory]
        [MemberData(nameof(SchemaEvents))]
        public async Task Should_clear_cache_for_id_after_update_event(IEvent @event)
        {
            var schemas = ProviderResults(schemaV1, schemaV2);

            repository.Setup(x => x.FindSchemaAsync(schemaId)).Returns(() => Task.FromResult<ISchemaEntityWithSchema>(schemas()));

            await ProvideSchemaById(schemaV1);
            await RaiseEvent(@event);
            await ProvideSchemaById(schemaV2);

            repository.Verify(x => x.FindSchemaAsync(schemaId), Times.Exactly(2));
        }

        [Theory]
        [MemberData(nameof(SchemaEvents))]
        public async Task Should_clear_cache_for_name_after_update_event(IEvent @event)
        {
            var schemas = ProviderResults(schemaV1, schemaV2);

            repository.Setup(x => x.FindSchemaAsync(appId, schemaName)).Returns(() => Task.FromResult<ISchemaEntityWithSchema>(schemas()));

            await ProvideSchemaByName(schemaV1);
            await RaiseEvent(@event);
            await ProvideSchemaByName(schemaV2);

            repository.Verify(x => x.FindSchemaAsync(appId, schemaName), Times.Exactly(2));
        }

        private async Task RaiseEvent(IEvent @event)
        {
            await sut.On(new Envelope<IEvent>(@event).SetAggregateId(schemaId).SetAppId(appId));
        }

        private async Task ProvideSchemaById(ISchemaEntityWithSchema schema)
        {
            Assert.Equal(schema, await sut.FindSchemaByIdAsync(schemaId));
        }

        private async Task ProvideSchemaByName(ISchemaEntityWithSchema schema)
        {
            Assert.Equal(schema, await sut.FindSchemaByNameAsync(appId, schemaName));
        }

        private static Func<T> ProviderResults<T>(params T[] items)
        {
            var index = 0;

            return () => items[index++];
        }

        public static IEnumerable<object[]> SchemaEvents
        {
            get
            {
                yield return new object[] { new SchemaDeleted() };
                yield return new object[] { new SchemaPublished() };
                yield return new object[] { new SchemaUnpublished() };
                yield return new object[] { new SchemaUpdated() };
                yield return new object[] { new FieldAdded() };
            }
        }
    }
}
