// ==========================================================================
//  IAppUserGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains
{
    public interface IAppUserGrain : IGrainWithStringKey
    {
        Task<List<string>> GetSchemaNamesAsync();

        Task AddAppAsync(string appName);

        Task RemoveAppAsync(string appName);
    }
}
