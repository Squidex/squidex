// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppUsageDeleter : IDeleter
{
    private readonly IApiUsageTracker apiUsageTracker;

    public AppUsageDeleter(IApiUsageTracker apiUsageTracker)
    {
        this.apiUsageTracker = apiUsageTracker;
    }

    public Task DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return apiUsageTracker.DeleteAsync(app.Id.ToString(), ct);
    }
}
