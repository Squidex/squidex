// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppLogStore
    {
        Task LogAsync(Guid appId, Instant timestamp, string? requestMethod, string? requestPath, string? userId, string? clientId, long elapsedMs, double costs);

        Task ReadLogAsync(Guid appId, DateTime fromDate, DateTime toDate, Stream stream, CancellationToken ct = default);
    }
}
