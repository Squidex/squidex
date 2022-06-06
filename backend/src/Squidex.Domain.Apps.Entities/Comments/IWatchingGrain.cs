// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public interface IWatchingGrain : IGrainWithStringKey
    {
        Task<string[]> GetWatchingUsersAsync(string resource, string userId);
    }
}
