﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Events;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppEventDeleter(IEventStore eventStore) : IDeleter
{
    public int Order => int.MaxValue;

    public Task DeleteAppAsync(App app,
        CancellationToken ct)
    {
        var streamFilter = StreamFilter.Prefix($"%-{app.Id}");

        return eventStore.DeleteAsync(streamFilter, ct);
    }
}
