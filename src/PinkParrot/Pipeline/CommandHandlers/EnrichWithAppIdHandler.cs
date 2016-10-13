// ==========================================================================
//  EnrichWithAppIdHandler.cs
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
