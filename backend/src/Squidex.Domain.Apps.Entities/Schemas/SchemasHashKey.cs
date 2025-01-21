// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Schemas;

public sealed class SchemasHashKey : ReadonlyDictionary<DomainId, long>
{
    public static readonly SchemasHashKey Empty = new SchemasHashKey(new Dictionary<DomainId, long>(), default);

    public Instant Timestamp { get; }

    private SchemasHashKey(IDictionary<DomainId, long> idVersions, Instant timestamp)
        : base(idVersions)
    {
        Timestamp = timestamp;
    }

    public static SchemasHashKey Create(App app, IEnumerable<Schema> schemas, Instant created = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(schemas);

        return Create(app, schemas.ToDictionary(x => x.Id, x => x.Version), created);
    }

    public static SchemasHashKey Create(App app, Dictionary<DomainId, long> schemas, Instant created = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(schemas);

        var idVersions = new Dictionary<DomainId, long>
        {
            [app.Id] = app.Version,
        };

        foreach (var (id, version) in schemas)
        {
            idVersions[id] = version;
        }

        if (created == default)
        {
            created = SystemClock.Instance.GetCurrentInstant();
        }

        return new SchemasHashKey(idVersions, created);
    }
}
