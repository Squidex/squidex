// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Diagnostics
{
    public interface IHealthCheck
    {
        IEnumerable<string> Scopes { get; }

        Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
