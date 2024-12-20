﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Schemas;

public sealed class BackupSchemas(Rebuilder rebuilder) : IBackupHandler
{
    private const int BatchSize = 100;
    private readonly HashSet<DomainId> schemaIds = [];

    public string Name { get; } = "Schemas";

    public Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context,
        CancellationToken ct)
    {
        switch (@event.Payload)
        {
            case SchemaCreated:
                schemaIds.Add(@event.Headers.AggregateId());
                break;
            case SchemaDeleted:
                schemaIds.Remove(@event.Headers.AggregateId());
                break;
        }

        return Task.FromResult(true);
    }

    public async Task RestoreAsync(RestoreContext context,
        CancellationToken ct)
    {
        if (schemaIds.Count > 0)
        {
            await rebuilder.InsertManyAsync<SchemaDomainObject, Schema>(schemaIds, BatchSize, ct);
        }
    }
}
