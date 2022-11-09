// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Counter;

public interface ICounterService
{
    Task<long> IncrementAsync(DomainId appId, string name,
        CancellationToken ct = default);

    Task<long> ResetAsync(DomainId appId, string name, long value,
        CancellationToken ct = default);
}
