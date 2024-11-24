// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ContentEventDeleter(IContentRepository contentRepository, IEventStore eventStore) : IDeleter
{
    public int Order => -1000;

    public Task DeleteAppAsync(App app, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public async Task DeleteSchemAsync(App app, Schema schema,
        CancellationToken ct)
    {
        await foreach (var id in contentRepository.StreamIds(app.Id, schema.Id, SearchScope.All, ct))
        {
            var streamFilter = StreamFilter.Prefix($"content-{DomainId.Combine(app.Id, id)}");

            await eventStore.DeleteAsync(streamFilter, ct);
        }
    }
}
