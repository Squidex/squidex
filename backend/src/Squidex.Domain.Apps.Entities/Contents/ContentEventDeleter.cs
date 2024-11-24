// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ContentEventDeleter(IEventStore eventStore) : IDeleter
{
    public int Order => int.MaxValue;

    public Task DeleteAppAsync(App app,
        CancellationToken ct)
    {
        var streamFilter = StreamFilter.Prefix($"content-{app.Id}");

        return eventStore.DeleteAsync(streamFilter, ct);
    }
}
