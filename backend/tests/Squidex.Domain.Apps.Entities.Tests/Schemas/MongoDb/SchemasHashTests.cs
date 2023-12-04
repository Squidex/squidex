// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Domain.Apps.Entities.Schemas.MongoDb;

[Trait("Category", "Dependencies")]
public class SchemasHashTests : GivenContext, IClassFixture<SchemasHashFixture>
{
    public SchemasHashFixture _ { get; }

    public SchemasHashTests(SchemasHashFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_compute_cache_independent_from_order()
    {
        var schema1 = Schema.WithId(DomainId.NewGuid(), "my-schema");
        var schema2 = Schema.WithId(DomainId.NewGuid(), "my-schema") with { Version = 3 };

        var hash1 = await _.SchemasHash.ComputeHashAsync(App, new[] { schema1, schema2 }, CancellationToken);
        var hash2 = await _.SchemasHash.ComputeHashAsync(App, new[] { schema2, schema1 }, CancellationToken);

        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public async Task Should_compute_cache_independent_from_db()
    {
        var schema1 = Schema.WithId(DomainId.NewGuid(), "my-schema");
        var schema2 = Schema.WithId(DomainId.NewGuid(), "my-schema") with { Version = 3 };

        var timestamp = SystemClock.Instance.GetCurrentInstant().WithoutMs();

        var computedHash = await _.SchemasHash.ComputeHashAsync(App, new[] { schema1, schema2 }, CancellationToken);

        await _.SchemasHash.On(new[]
        {
            Envelope.Create<IEvent>(new SchemaCreated
            {
                AppId = AppId,
                SchemaId = schema1.NamedId()
            }).SetEventStreamNumber(schema1.Version).SetTimestamp(timestamp),

            Envelope.Create<IEvent>(new SchemaCreated
            {
                AppId = AppId,
                SchemaId = schema2.NamedId()
            }).SetEventStreamNumber(schema2.Version).SetTimestamp(timestamp)
        });

        var (dbTime, dbHash) = await _.SchemasHash.GetCurrentHashAsync(App, CancellationToken);

        Assert.Equal(dbHash, computedHash);
        Assert.Equal(dbTime, timestamp);
    }
}
