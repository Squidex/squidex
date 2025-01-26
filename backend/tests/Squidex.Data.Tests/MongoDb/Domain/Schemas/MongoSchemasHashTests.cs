// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.MongoDb.TestHelpers;

namespace Squidex.MongoDb.Domain.Schemas;

[Trait("Category", "TestContainer")]
public class MongoSchemasHashTests(MongoFixture fixture) : GivenContext, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoSchemasHash sut = new MongoSchemasHash(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Should_compute_cache_independent_from_db()
    {
        var schema1 = Schema.WithId(DomainId.NewGuid(), "my-schema");
        var schema2 = Schema.WithId(DomainId.NewGuid(), "my-schema") with { Version = 3 };

        var timestamp = SystemClock.Instance.GetCurrentInstant().WithoutMs();

        await sut.On(
        [
            Envelope.Create<IEvent>(new SchemaCreated
            {
                AppId = AppId,
                SchemaId = schema1.NamedId(),
            }).SetEventStreamNumber(schema1.Version).SetTimestamp(timestamp),

            Envelope.Create<IEvent>(new SchemaCreated
            {
                AppId = AppId,
                SchemaId = schema2.NamedId(),
            }).SetEventStreamNumber(schema2.Version).SetTimestamp(timestamp),
        ]);

        var hashKey = await sut.GetCurrentHashAsync(App, CancellationToken);

        Assert.NotEmpty(hashKey);
        Assert.Equal(hashKey.Timestamp, timestamp);
    }
}
