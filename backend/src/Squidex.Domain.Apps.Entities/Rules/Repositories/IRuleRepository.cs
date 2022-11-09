// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Repositories;

public interface IRuleRepository
{
    Task<List<IRuleEntity>> QueryAllAsync(DomainId appId,
        CancellationToken ct = default);
}
