// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppUsageDeleter(IApiUsageTracker apiUsageTracker) : IDeleter
{
    public Task DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return apiUsageTracker.DeleteAsync(app.Id.ToString(), ct);
    }
}
