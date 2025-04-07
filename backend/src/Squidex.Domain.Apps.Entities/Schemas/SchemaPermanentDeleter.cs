// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Schemas;

public sealed class SchemaPermanentDeleter(
    IAppProvider appProvider,
    IEnumerable<IDeleter> deleters,
    IOptions<SchemasOptions> options,
    IDomainObjectFactory factory,
    TypeRegistry typeRegistry)
    : IEventConsumer
{
    private readonly IEnumerable<IDeleter> deleters = deleters.OrderBy(x => x.Order).ToList();
    private readonly SchemasOptions options = options.Value;
    private readonly HashSet<string> consumingTypes =
        [
            typeRegistry.GetName<IEvent, SchemaDeleted>(),
        ];

    public StreamFilter EventsFilter { get; } = StreamFilter.Prefix("schema-");

    public ValueTask<bool> HandlesAsync(StoredEvent @event)
    {
        return new ValueTask<bool>(consumingTypes.Contains(@event.Data.Type));
    }

    public async Task On(Envelope<IEvent> @event)
    {
        if (@event.Headers.Restored())
        {
            return;
        }

        switch (@event.Payload)
        {
            case SchemaDeleted schemaDeleted:
                await OnDeleteAsync(schemaDeleted);
                break;
        }
    }

    private async Task OnDeleteAsync(SchemaDeleted schemaDeleted)
    {
        // The user can either remove the app itself or via a global setting for all apps.
        if (!schemaDeleted.Permanent && !options.DeletePermanent)
        {
            return;
        }

        using var activity = Telemetry.Activities.StartActivity("RemoveAppFromSystem");

        var app = await appProvider.GetAppAsync(schemaDeleted.AppId.Id);
        if (app == null)
        {
            return;
        }

        var schema = await GetSchemaAsync(app.Id, schemaDeleted.SchemaId.Id);
        if (schema == null)
        {
            return;
        }

        foreach (var deleter in deleters)
        {
            using (Telemetry.Activities.StartActivity(deleter.GetType().Name))
            {
                await deleter.DeleteSchemaAsync(app, schema.Snapshot, default);
            }
        }
    }

    private async Task<SchemaDomainObject?> GetSchemaAsync(DomainId appId, DomainId schemaId)
    {
        // Bypass our normal resolve process, so that we can also retrieve the deleted schema.
        var schema = factory.Create<SchemaDomainObject>(DomainId.Combine(appId, schemaId));

        await schema.EnsureLoadedAsync();
        // If the app does not exist, the version is lower than zero.
        return schema.Version < 0 ? null : schema;
    }
}
