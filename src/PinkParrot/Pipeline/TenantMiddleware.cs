// ==========================================================================
//  TenantMiddleware.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PinkParrot.Read.Services;

namespace PinkParrot.Pipeline
{
    public sealed class TenantMiddleware
    {
        private readonly ITenantProvider tenantProvider;
        private readonly RequestDelegate next;

        public TenantMiddleware(RequestDelegate next, ITenantProvider tenantProvider)
        {
            this.next = next;

            this.tenantProvider = tenantProvider;
        }

        private class TenantFeature : ITenantFeature
        {
            public Guid TenantId { get; set; }
        }

        public async Task Invoke(HttpContext context)
        {
            var tenantId = await tenantProvider.ProvideTenantIdByDomainAsync(context.Request.Host.ToString());

            if (tenantId.HasValue)
            {
                context.Features.Set<ITenantFeature>(new TenantFeature { TenantId = tenantId.Value });
            }

            await next(context);
        }
    }
}
