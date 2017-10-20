﻿// ==========================================================================
//  EnrichWithAppIdHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Write;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public sealed class EnrichWithAppIdCommandMiddleware : ICommandMiddleware
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithAppIdCommandMiddleware(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is AppCommand appCommand && appCommand.AppId == null)
            {
                var appFeature = httpContextAccessor.HttpContext.Features.Get<IAppFeature>();

                if (appFeature == null)
                {
                    throw new InvalidOperationException("Cannot resolve app.");
                }

                appCommand.AppId = new NamedId<Guid>(appFeature.App.Id, appFeature.App.Name);
            }

            return next();
        }
    }
}
