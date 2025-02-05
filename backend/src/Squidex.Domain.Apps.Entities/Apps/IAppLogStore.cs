// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps;

public interface IAppLogStore
{
    Task LogAsync(DomainId appId, RequestLog request,
        CancellationToken ct = default);

    Task ReadLogAsync(DomainId appId, Instant fromTime, Instant toTime, Stream stream,
        CancellationToken ct = default);
}
