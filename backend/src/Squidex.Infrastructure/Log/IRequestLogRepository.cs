// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.Log;

public interface IRequestLogRepository
{
    Task InsertManyAsync(IEnumerable<Request> items,
        CancellationToken ct = default);

    Task DeleteAsync(string key,
        CancellationToken ct = default);

    IAsyncEnumerable<Request> QueryAllAsync(string key, Instant fromTime, Instant toTime,
        CancellationToken ct = default);
}
