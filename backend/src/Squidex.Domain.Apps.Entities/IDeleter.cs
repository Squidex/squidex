// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities
{
    public interface IDeleter
    {
        int Order => 0;

        Task DeleteAppAsync(IAppEntity app,
            CancellationToken ct);

        Task DeleteContributorAsync(DomainId appId, string contributorId,
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
