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
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppLogStore
    {
        Task LogAsync(DomainId appId, RequestLog request,
            CancellationToken ct = default);

        Task ReadLogAsync(DomainId appId, DateTime fromDate, DateTime toDate, Stream stream,
            CancellationToken ct = default);
    }
}
