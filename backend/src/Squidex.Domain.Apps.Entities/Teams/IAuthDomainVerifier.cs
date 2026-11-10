// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams;

public interface IAuthDomainVerifier
{
    Task<bool> IsVerifiedAsync(string domain, DomainId teamId,
        CancellationToken ct = default);
}
