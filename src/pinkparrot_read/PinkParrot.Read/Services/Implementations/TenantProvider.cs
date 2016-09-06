// ==========================================================================
//  TenantProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace PinkParrot.Read.Services.Implementations
{
    public sealed class TenantProvider : ITenantProvider
    {
        public Task<Guid?> ProvideTenantIdByDomainAsync(string domain)
        {
            return Task.FromResult<Guid?>(Guid.Empty);
        }
    }
}
