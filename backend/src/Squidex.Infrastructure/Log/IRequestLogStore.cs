// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Log
{
    public interface IRequestLogStore
    {
        bool IsEnabled { get; }

        Task LogAsync(Request request,
            CancellationToken ct = default);

        Task DeleteAsync(string key,
            CancellationToken ct = default);

        IAsyncEnumerable<Request> QueryAllAsync(string key, DateTime fromDate, DateTime toDate,
            CancellationToken ct = default);
    }
}
