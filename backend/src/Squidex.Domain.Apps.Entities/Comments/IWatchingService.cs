// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public interface IWatchingService
    {
        Task<string[]> GetWatchingUsersAsync(DomainId appId, string resource, string userId);
    }
}
