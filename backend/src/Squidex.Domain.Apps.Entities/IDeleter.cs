// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities;

public interface IDeleter
{
    int Order => 0;

    Task DeleteAppAsync(App app,
        CancellationToken ct);

    Task DeleteSchemaAsync(App app, Schema schema,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    Task DeleteContributorAsync(DomainId appId, string contributorId,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
