// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Domain.Apps.Entities.Schemas.MongoDb
{
    [Trait("Category", "Dependencies")]
    public class SchemasHashTests : IClassFixture<SchemasHashFixture>
    {
        public SchemasHashFixture _ { get; }

        public SchemasHashTests(SchemasHashFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_compute_cache_independent_from_order()
        {
            var app = CreateApp(DomainId.NewGuid(), 1);

            var schema1 = CreateSchema(DomainId.NewGuid(), 2);
            var schema2 = CreateSchema(DomainId.NewGuid(), 3);

            var hash1 = await _.SchemasHash.ComputeHashAsync(app, new[] { schema1, schema2 });
            var hash2 = await _.SchemasHash.ComputeHashAsync(app, new[] { schema2, schema1 });

            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public async Task Should_compute_cache_independent_from_db()
        {
            var app = CreateApp(DomainId.NewGuid(), 1);

            var schema1 = CreateSchema(DomainId.NewGuid(), 2);
            var schema2 = CreateSchema(DomainId.NewGuid(), 3);

            var timestamp = SystemClock.Instance.GetCurrentInstant().WithoutMs();

            var computedHash = await _.SchemasHash.ComputeHashAsync(app, new[] { schema1, schema2 });

            await _.SchemasHash.On(new[]
            {
                Envelope.Create<IEvent>(new SchemaCreated
                {
                    AppId = NamedId.Of(app.Id, "my-app"),
                    SchemaId = NamedId.Of(schema1.Id, "my-schema")
                }).SetEventStreamNumber(schema1.Version).SetTimestamp(timestamp),

                Envelope.Create<IEvent>(new SchemaCreated
                {
                    AppId = NamedId.Of(app.Id, "my-app"),
                    SchemaId = NamedId.Of(schema2.Id, "my-schema")
                }).SetEventStreamNumber(schema2.Version).SetTimestamp(timestamp)
            });

            var (dbTime, dbHash) = await _.SchemasHash.GetCurrentHashAsync(app);

            Assert.Equal(dbHash, computedHash);
            Assert.Equal(dbTime, timestamp);
        }

        private static IAppEntity CreateApp(DomainId id, long version)
        {
            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.Id)
                .Returns(id);
            A.CallTo(() => app.Version)
                .Returns(version);

            return app;
        }

        private static ISchemaEntity CreateSchema(DomainId id, long version)
        {
            var schema = A.Fake<ISchemaEntity>();

            A.CallTo(() => schema.Id)
                .Returns(id);
            A.CallTo(() => schema.Version)
                .Returns(version);

            return schema;
        }
    }
}
