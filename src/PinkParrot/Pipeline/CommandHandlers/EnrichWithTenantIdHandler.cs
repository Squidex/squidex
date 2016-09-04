// ==========================================================================
//  EnrichWithTenantIdHandler.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Read.Services;
using PinkParrot.Write;

// ReSharper disable InvertIf

namespace PinkParrot.Pipeline.CommandHandlers
{
    public sealed class EnrichWithTenantIdHandler : ICommandHandler
    {
        private readonly ITenantProvider tenantProvider;
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithTenantIdHandler(ITenantProvider tenantProvider, IHttpContextAccessor httpContextAccessor)
        {
            this.tenantProvider = tenantProvider;

            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HandleAsync(CommandContext context)
        {
            var tenantCommand = context.Command as ITenantCommand;

            if (tenantCommand != null)
            {
                var domain = httpContextAccessor.HttpContext.Request.Host.ToString();

                tenantCommand.TenantId = await tenantProvider.ProvideTenantIdByDomainAsync(domain);
            }

            return false;
        }
    }
}
