// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppEventDeleter : IDeleter
{
    private readonly IEventStore eventStore;

    public int Order => int.MaxValue;

    public AppEventDeleter(IEventStore eventStore)
    {
        this.eventStore = eventStore;
    }

    public Task DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        var streamFilter = StreamFilter.Prefix($"([a-zA-Z0-9]+)-{app.Id}");

        return eventStore.DeleteAsync(streamFilter, ct);
    }
}
