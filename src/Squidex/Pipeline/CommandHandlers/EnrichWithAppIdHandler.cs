// ==========================================================================
//  EnrichWithAppIdHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Write;

// ReSharper disable InvertIf

namespace Squidex.Pipeline.CommandHandlers
{
    public sealed class EnrichWithAppIdHandler : ICommandHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithAppIdHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var appCommand = context.Command as IAppCommand;

            if (appCommand != null)
            {
                var appFeature = httpContextAccessor.HttpContext.Features.Get<IAppFeature>();

                if (appFeature == null)
                {
                    throw new InvalidOperationException("Cannot resolve app");
                }

                appCommand.AppId = appFeature.AppId;
            }

            return Task.FromResult(false);
        }
    }
}
