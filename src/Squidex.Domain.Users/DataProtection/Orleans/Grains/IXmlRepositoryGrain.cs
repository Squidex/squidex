// ==========================================================================
//  IXmlRepositoryGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Domain.Users.DataProtection.Orleans.Grains
{
    public interface IXmlRepositoryGrain : IGrainWithStringKey
    {
        Task<string[]> GetAllElementsAsync();

        Task StoreElementAsync(string element, string friendlyName);
    }
}
