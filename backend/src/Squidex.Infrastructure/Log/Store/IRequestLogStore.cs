﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Log.Store
{
    public interface IRequestLogStore
    {
        Task LogAsync(Request request);

        Task QueryAllAsync(Func<Request, Task> callback, string key, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    }
}
