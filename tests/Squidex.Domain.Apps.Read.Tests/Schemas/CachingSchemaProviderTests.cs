// ==========================================================================
//  CachingSchemaProviderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Read.Schemas.Services.Implementations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

namespace Squidex.Domain.Apps.Read.Schemas
{
    public class CachingSchemaProviderTests
    {
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly ISchemaRepository repository = A.Fake<ISchemaRepository>();
        private readonly CachingSchemaProvider sut;
        private readonly ISchemaEntity schemaV1 = A.Dummy<ISchemaEntity>();
        private readonly ISchemaEntity schemaV2 = A.Dummy<ISchemaEntity>();
        private readonly NamedId<Guid> schemaId = new NamedId<Guid>(Guid.NewGuid(), "my-schema");
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");

        public CachingSchemaProviderTests()
        {
            A.CallTo(() => schemaV1.Id).Returns(schemaId.Id);
            A.CallTo(() => schemaV1.Name).Returns(schemaId.Name);
            A.CallTo(() => schemaV1.AppId).Returns(appId.Id);

            A.CallTo(() => schemaV2.Id).Returns(schemaId.Id);
            A.CallTo(() => schemaV2.Name).Returns(schemaId.Name);
            A.CallTo(() => schemaV2.AppId).Returns(appId.Id);

            sut = new CachingSchemaProvider(cache, repository);
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
            A.CallTo(() => repository.FindSchemaAsync(schemaId.Id))
                .Returns(schemaV1);

            await ProvideSchemaById(schemaV1);
            await ProvideSchemaByName(schemaV1);

            A.CallTo(() => repository.FindSchemaAsync(schemaId.Id)).MustHaveHappened();
            A.CallTo(() => repository.FindSchemaAsync(appId.Id, schemaId.Name)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_also_retrieve_schema_by_id_if_retrieved_by_name_before()
        {
            A.CallTo(() => repository.FindSchemaAsync(appId.Id, schemaId.Name))
                .Returns(schemaV1);

            await ProvideSchemaByName(schemaV1);
            await ProvideSchemaById(schemaV1);

            A.CallTo(() => repository.FindSchemaAsync(schemaId.Id)).MustNotHaveHappened();
            A.CallTo(() => repository.FindSchemaAsync(appId.Id, schemaId.Name)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_clear_cache_for_id_after_update_event()
        {
            A.CallTo(() => repository.FindSchemaAsync(schemaId.Id))
                .Returns(schemaV2);
            A.CallTo(() => repository.FindSchemaAsync(schemaId.Id))
                .Returns(schemaV1).Once();

            await ProvideSchemaById(schemaV1);

            sut.On(Envelope.Create(new FieldAdded { AppId = appId, SchemaId = schemaId })).Wait();

            await ProvideSchemaById(schemaV2);

            A.CallTo(() => repository.FindSchemaAsync(schemaId.Id)).MustHaveHappened(Repeated.Exactly.Times(2));
        }

        [Fact]
        public async Task Should_clear_cache_for_name_after_update_event()
        {
            A.CallTo(() => repository.FindSchemaAsync(appId.Id, schemaId.Name))
                .Returns(schemaV2);
            A.CallTo(() => repository.FindSchemaAsync(appId.Id, schemaId.Name))
                .Returns(schemaV1).Once();

            await ProvideSchemaByName(schemaV1);

            sut.On(Envelope.Create(new SchemaUpdated { AppId = appId, SchemaId = schemaId })).Wait();

            await ProvideSchemaByName(schemaV2);

            A.CallTo(() => repository.FindSchemaAsync(appId.Id, schemaId.Name)).MustHaveHappened(Repeated.Exactly.Times(2));
        }

        private async Task ProvideSchemaById(ISchemaEntity schema)
        {
            Assert.Equal(schema, await sut.FindSchemaByIdAsync(schemaId.Id));
        }

        private async Task ProvideSchemaByName(ISchemaEntity schema)
        {
            Assert.Equal(schema, await sut.FindSchemaByNameAsync(appId.Id, schemaId.Name));
        }
    }
}
