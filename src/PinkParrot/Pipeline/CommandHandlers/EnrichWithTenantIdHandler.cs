// ==========================================================================
//  EnrichWithTenantIdHandler.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Write;

// ReSharper disable InvertIf

namespace PinkParrot.Pipeline.CommandHandlers
{
    public sealed class EnrichWithTenantIdHandler : ICommandHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithTenantIdHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var tenantCommand = context.Command as ITenantCommand;

            if (tenantCommand != null)
            {
                var tenantFeature = httpContextAccessor.HttpContext.Features.Get<ITenantFeature>();

                if (tenantFeature == null)
                {
                    throw new InvalidOperationException("Cannot reslolve tenant");
                }

                tenantCommand.TenantId = tenantFeature.TenantId;
            }

            return Task.FromResult(false);
        }
    }
}
