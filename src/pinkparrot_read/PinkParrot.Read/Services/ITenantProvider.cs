// ==========================================================================
//  ITenantProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace PinkParrot.Read.Services
{
    public interface ITenantProvider
    {
        Task<Guid?> ProvideTenantIdByDomainAsync(string domain);
    }
}
