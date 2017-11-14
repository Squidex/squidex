// ==========================================================================
//  XmlRepositoryGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;

namespace Squidex.Domain.Users.DataProtection.Orleans.Grains.Implementations
{
    public sealed class XmlRepositoryGrain : Grain<Dictionary<string, string>>, IXmlRepositoryGrain
    {
        public Task<string[]> GetAllElementsAsync()
        {
            return Task.FromResult(State.Values.ToArray());
        }

        public Task StoreElementAsync(string element, string friendlyName)
        {
            State[friendlyName] = element;

            return WriteStateAsync();
        }
    }
}
